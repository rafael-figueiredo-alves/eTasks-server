# Offline-First Client Sync

## Objetivo

Este documento define:

1. a estrutura recomendada da `Outbox` do cliente
2. a politica de resolucao de conflitos

O objetivo e manter um modelo unico que funcione para:

- Blazor WebAssembly + PWA
- Delphi Windows
- Delphi Android

O cliente deve funcionar com banco local como fonte primaria de leitura, e a API deve ser usada para sincronizacao.

## Principio Base

O cliente nunca depende da rede para concluir a acao do usuario.

Fluxo:

1. usuario cria, edita, conclui ou exclui localmente
2. a alteracao e persistida no banco local imediatamente
3. a mutacao entra na `Outbox`
4. quando houver conectividade, o cliente envia a `Outbox` para `POST /api/v2/tasks/push-sync`
5. depois executa `POST /api/v2/tasks/sync`
6. aplica `upserts` e `deleted tombstones` no banco local
7. atualiza o cursor de sincronizacao

## Tabelas Locais Minimas

Recomendacao:

- `tasks_local`
- `tasks_outbox`
- `sync_state`

### `tasks_local`

Representa o snapshot local das tarefas.

Campos recomendados:

| Campo | Tipo sugerido | Uso |
|---|---|---|
| `id` | string/guid | id da tarefa |
| `user_uid` | string/guid | dono da tarefa |
| `generated_from_task_id` | string/guid nullable | origem de recorrencia |
| `summary` | string | resumo |
| `notes` | string nullable | anotacoes |
| `priority` | int | `Low`, `Normal`, `Medium`, `Urgent` |
| `task_date` | datetime | data da tarefa |
| `is_completed` | bool | status |
| `completed_at` | datetime nullable | conclusao |
| `created_at` | datetime | criacao |
| `updated_at` | datetime nullable | atualizacao |
| `is_deleted` | bool | tombstone local |
| `deleted_at` | datetime nullable | remocao logica |
| `server_etag` | string nullable | ultimo ETag conhecido |
| `sync_status` | string | `synced`, `pending`, `conflict`, `error` |
| `last_local_change_at` | datetime | auditoria local |
| `last_sync_at` | datetime nullable | ultima confirmacao do servidor |

### `tasks_outbox`

Representa as mutacoes pendentes.

Campos recomendados:

| Campo | Tipo sugerido | Uso |
|---|---|---|
| `id` | string/guid | id interno da linha da outbox |
| `client_mutation_id` | string | id unico da mutacao enviada ao servidor |
| `task_id` | string/guid nullable | recurso afetado |
| `operation` | string/int | `Create`, `Update`, `SetCompletion`, `Delete` |
| `payload_json` | text | payload exato a enviar |
| `expected_etag` | string nullable | ETag esperado no momento da mutacao |
| `created_at` | datetime | quando entrou na fila |
| `last_attempt_at` | datetime nullable | ultima tentativa de envio |
| `attempt_count` | int | numero de tentativas |
| `status` | string | `pending`, `processing`, `conflict`, `failed`, `done` |
| `last_error_code` | string nullable | codigo do ultimo erro |
| `last_error_message` | text nullable | mensagem da ultima falha |
| `depends_on_mutation_id` | string nullable | quando houver dependencia entre operacoes |

### `sync_state`

Estado global de sincronizacao por recurso.

Campos recomendados:

| Campo | Tipo sugerido | Uso |
|---|---|---|
| `resource_name` | string | ex: `tasks` |
| `last_server_cursor` | datetime nullable | valor de `ServerTime` da ultima sync bem sucedida |
| `last_sync_started_at` | datetime nullable | diagnostico |
| `last_sync_finished_at` | datetime nullable | diagnostico |
| `last_sync_status` | string | `success`, `partial`, `failed` |
| `last_error_message` | text nullable | ultimo erro |

## Formato da Outbox

Cada linha da `Outbox` deve representar uma intencao do usuario, nao um delta tecnico da UI.

Exemplos:

- criar tarefa
- atualizar tarefa
- marcar como concluida
- excluir logicamente

Evite gravar eventos como:

- clicou no checkbox
- abriu modal
- alterou texto temporariamente

Grave apenas mutacoes de dominio.

## Regras de Gravacao na Outbox

### Create

Quando criar offline:

1. gerar `task_id` no cliente
2. inserir a tarefa em `tasks_local`
3. inserir item na `tasks_outbox` com `operation=Create`
4. `expected_etag = null`

### Update

Quando editar:

1. atualizar `tasks_local`
2. adicionar item na `tasks_outbox` com `operation=Update`
3. copiar o `server_etag` atual para `expected_etag`

### SetCompletion

Quando concluir ou desfazer conclusao:

1. atualizar `tasks_local`
2. adicionar item na `tasks_outbox` com `operation=SetCompletion`
3. copiar `server_etag` para `expected_etag`

### Delete

Quando excluir:

1. marcar `is_deleted=true` e `deleted_at` em `tasks_local`
2. adicionar item na `tasks_outbox` com `operation=Delete`
3. copiar `server_etag` para `expected_etag`

## Consolidacao de Outbox

Antes de enviar ao servidor, o cliente pode compactar mutacoes pendentes da mesma tarefa.

Politica recomendada:

- `Create + Update` vira um unico `Create` com payload final
- `Create + SetCompletion` vira um unico `Create` com estado final
- `Create + Delete` cancela tudo e remove a tarefa local se ela nunca foi sincronizada
- `Update + Update` vira um unico `Update` com payload final
- `Update + SetCompletion` mantem ordem, a menos que seu payload final possa absorver o estado
- `Delete` sempre vence operacoes anteriores ainda nao enviadas

Nao compacte se isso quebrar a rastreabilidade ou a ordem de negocio.

## Ordem de Processamento

O cliente deve enviar a `Outbox` em ordem cronologica.

Prioridade:

1. mutacoes dependentes do mesmo recurso na ordem em que ocorreram
2. operacoes `Create` antes de `Update`/`Delete` do mesmo `task_id`
3. lote pequeno e previsivel, por exemplo `20` a `100` mutacoes por envio

## Politica de Resolucao de Conflitos

### Conflito Base

Um conflito ocorre quando:

- o cliente envia `expected_etag`
- o servidor calcula outro estado para o recurso
- a API devolve `Conflict`

Esse e o comportamento correto para cenarios offline-first.

### Politica Recomendada

Use `manual merge leve` com suporte a `retry orientado`.

Pratica:

1. cliente tenta aplicar `push-sync`
2. se houver `Conflict`, ele nao sobrescreve automaticamente
3. cliente baixa o estado atual com `sync` ou `get`
4. cliente marca o item local como `conflict`
5. UI mostra que existe divergencia e pede escolha do usuario

### Estrategia por Tipo de Campo

#### Campos simples

Campos:

- `summary`
- `notes`
- `priority`
- `task_date`

Politica:

- se servidor e cliente mudaram o mesmo campo desde o ultimo `etag`, tratar como conflito real

#### Campos de status

Campos:

- `is_completed`
- `completed_at`

Politica:

- se o cliente mandou concluir e o servidor tambem concluiu, aceitar servidor
- se um concluiu e o outro desfez, tratar como conflito

#### Exclusao logica

Politica:

- se o servidor ja removeu logicamente, o cliente deve aceitar o tombstone do servidor
- se o cliente tentou editar algo que o servidor ja removeu, tratar como `not_found` ou `conflict`, dependendo do caso

### Politicas de Resolucao na UI

Recomendacao de tres opcoes:

1. `Usar servidor`
2. `Manter minha versao`
3. `Mesclar manualmente`

#### Usar servidor

- descarta alteracao local pendente
- atualiza banco local com o estado do servidor
- limpa a mutacao da outbox

#### Manter minha versao

- atualiza `expected_etag` para o ETag novo do servidor
- gera nova mutacao baseada no estado local atual
- reenfileira para novo envio

#### Mesclar manualmente

- mostra campo a campo
- salva resultado final local
- gera nova mutacao com base no merge

## Politica Automatizada Minima

Se quiser evitar UI de conflito no inicio:

- `notes`: pode usar merge por concatenacao ou preferencia do cliente
- `is_completed`: preferir o estado mais recente pelo timestamp local da mutacao, se confiavel
- `summary`, `priority`, `task_date`: exigir resolucao manual

Esse meio-termo costuma ser suficiente no MVP.

## Fluxo de Sync Recomendado

### Ao recuperar conectividade

1. travar o sync por recurso para evitar concorrencia local
2. ler lote `pending` da outbox
3. enviar `push-sync`
4. processar resultados:
   - `Applied`: atualizar `tasks_local`, `server_etag`, limpar item da outbox
   - `Conflict`: marcar `tasks_local.sync_status = conflict`
   - `ValidationError`: marcar como erro funcional, nao reenfileirar automaticamente
   - `Failed`: manter para retry com backoff
5. executar `sync`
6. aplicar `upserts`
7. aplicar `deleted`
8. atualizar `sync_state.last_server_cursor`

### Backoff

Recomendacao:

- 1a falha: 30s
- 2a falha: 2min
- 3a falha: 10min
- depois: exponencial com teto

## Regras Importantes para Delphi e C#

### Delphi

- use SQLite local
- outbox como tabela real, nao apenas lista em memoria
- serialize `payload_json` com JSON estavel
- sempre persista antes de disparar thread de sync

### C# / Blazor PWA

- IndexedDB ou SQLite via WASM, se viavel no stack escolhido
- use service de sync desacoplado da UI
- evite estado local apenas em memoria

## Recomendacao Final

Para o eTasks, o melhor padrao e:

1. `Local-first UI`
2. `Outbox pattern`
3. `Pull sync incremental`
4. `Soft-delete com tombstones`
5. `ETag para concorrencia otimista`
6. `Conflito manual quando alterar o mesmo campo critico`

Isso entrega um modelo simples, robusto e reutilizavel em todas as plataformas do ecossistema.
