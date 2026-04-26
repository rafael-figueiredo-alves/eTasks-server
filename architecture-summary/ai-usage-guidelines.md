# Diretrizes de Uso de IA no eTasks

## Objetivo

Usar IA no eTasks para aumentar clareza, organizacao e capacidade de execucao do usuario, sem transformar o sistema em um chat genérico.

## Principio

A IA deve agregar valor sobre os dados que o usuario ja possui no sistema.

Ela deve:

- reduzir friccao
- ajudar a decidir
- ajudar a planejar
- resumir e reorganizar conteudo
- sugerir proximos passos

Ela nao deve:

- inventar fatos
- agir como autoridade definitiva em temas sensiveis
- substituir o criterio do usuario
- gerar texto longo sem utilidade pratica

## Bons usos por recurso

### Tarefas

- quebrar tarefas grandes
- reescrever tarefas vagas em acoes objetivas
- sugerir prioridade
- sugerir proximo passo quando houver bloqueio

### Metas

- transformar metas abstratas em plano concreto
- sugerir marcos
- apontar riscos de meta irrealista
- ajudar a definir criterio de sucesso

### Anotacoes

- resumir texto longo
- limpar rascunhos
- extrair checklist
- reorganizar ideias

### Leituras

- gerar resumo do que foi lido
- extrair aprendizados
- sugerir reflexoes
- sugerir proxima leitura ou continuidade

### Compras

- agrupar itens
- detectar duplicidade
- sugerir melhor organizacao da lista
- apoiar planejamento antes da compra

### Financas

- resumir comportamento financeiro
- apontar concentracao de gastos
- sugerir perguntas para revisao mensal
- ajudar na organizacao de categorias

Importante:

- IA em financas deve ser educacional e organizacional
- nao deve fornecer recomendacao de investimento personalizada

## Guardrails

- sempre deixar claro quando faltar contexto
- preferir respostas curtas e acionaveis
- manter foco no recurso atual
- respeitar o escopo dos dados do sistema
- evitar inferencias invasivas sobre o usuario

## Descoberta pela API

A API expoe `GET /api/v2/ai/capabilities` para que os clientes descubram:

- usos recomendados
- intents suportadas
- guardrails por recurso
- template de payload recomendado por recurso
- exemplos de `UserPrompt`

Isso permite que apps Delphi, PWA e outros clientes construam UI orientada ao contexto sem hardcode disperso.

## Contrato recomendado de payload

Os clientes devem preferir:

- `ResourceTitle`: identificador curto e legivel do recurso atual
- `ResourceContent`: snapshot textual mais relevante daquele recurso
- `AdditionalContext`: filtros, objetivo do usuario, restricoes e contexto da tela
- `UserPrompt`: pergunta ou pedido direto do usuario

### Exemplos por recurso

#### Tarefas

- `ResourceTitle`: resumo da tarefa
- `ResourceContent`: notas, prioridade, data, conclusao
- `AdditionalContext`: filtro atual, contexto da lista, objetivo do usuario

#### Metas

- `ResourceTitle`: resumo da meta
- `ResourceContent`: descricao, tipo, prioridade, status, recompensa
- `AdditionalContext`: prazo, restricoes, motivacao

#### Anotacoes

- `ResourceTitle`: assunto
- `ResourceContent`: conteudo integral
- `AdditionalContext`: formato desejado da resposta

#### Leituras

- `ResourceTitle`: titulo da leitura
- `ResourceContent`: autores, resumo, opiniao, progresso, status
- `AdditionalContext`: trecho recente, objetivo da leitura, duvida do usuario

#### Compras

- `ResourceTitle`: nome da lista
- `ResourceContent`: itens, local, tipo, status
- `AdditionalContext`: orcamento, evento, pessoas envolvidas

#### Financas

- `ResourceTitle`: periodo ou mes
- `ResourceContent`: resumo mensal, lancamentos e categorias
- `AdditionalContext`: objetivo do usuario e limites da analise

#### Perfil e uso

- `ResourceTitle`: usuario ou tela atual
- `ResourceContent`: configuracoes, bonus, resumo de uso
- `AdditionalContext`: pergunta do usuario sobre organizacao pessoal
