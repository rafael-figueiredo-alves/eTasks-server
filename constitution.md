# constitution.md

## Objetivo

Este arquivo define a constituicao de engenharia do repositorio. Ele deve concentrar principios duradouros, regras operacionais e criterios de qualidade usados para orientar humanos e agentes.

Use este documento para registrar:

- principios do projeto
- limites arquiteturais
- contratos que nao podem ser quebrados
- padroes de implementacao
- criterios de revisao
- requisitos de seguranca, testes e release

## Como Preencher

Recomendacoes para manter o documento util:

- escreva regras objetivas e verificaveis
- separe regra obrigatoria de recomendacao
- cite caminhos de arquivo e exemplos reais quando possivel
- documente decisoes estaveis, nao detalhes temporarios
- atualize este arquivo quando a arquitetura mudar de forma relevante

## 1. North Star

Preencha aqui a direcao principal do sistema.

Sugestoes:

- qual problema o produto resolve
- quais prioridades vencem tradeoffs
- quais atributos sao inegociaveis

Template:

```md
## 1. North Star

- Missao:
- Prioridades:
- Nao negociaveis:
```

## 2. Principios de Engenharia

Registre principios amplos que devem orientar toda mudanca.

Exemplos de topicos:

- simplicidade antes de generalizacao
- manter compatibilidade de contratos publicos
- evitar duplicacao de regra de negocio
- preferir clareza a "magia"

Template:

```md
## 2. Principios de Engenharia

- Principio:
  Regra:
  Impacto esperado:
```

## 3. Mapa Arquitetural

Descreva os blocos principais do sistema e os limites entre eles.

Exemplos de topicos:

- responsabilidade de cada projeto
- fluxos permitidos entre camadas
- dependencias proibidas
- pontos unicos de entrada

Template:

```md
## 3. Mapa Arquitetural

- Projeto:
  Responsabilidade:
  Pode depender de:
  Nao deve depender de:
```

## 4. Regras de Dominio

Registre aqui invariantes de negocio que nao podem ser quebradas.

Exemplos:

- estados validos de entidades
- requisitos de autenticacao/autorizacao
- regras de versao
- regras de auditoria ou log

Template:

```md
## 4. Regras de Dominio

- Regra:
  Motivacao:
  Onde validar:
```

## 5. Contratos Publicos

Liste contratos que exigem cuidado extra para evitar regressao.

Exemplos:

- endpoints da API
- DTOs compartilhados
- codigos de erro
- formato de cookies ou tokens

Template:

```md
## 5. Contratos Publicos

- Contrato:
  Consumidores:
  Compatibilidade exigida:
  Arquivos criticos:
```

## 6. Seguranca

Use esta secao para consolidar politicas de seguranca.

Exemplos:

- esquemas de autenticacao suportados
- politicas de autorizacao
- tratamento de antiforgery
- requisitos de cookies, JWT e segredos
- regras de validacao de entrada

Template:

```md
## 6. Seguranca

- Tema:
  Regra obrigatoria:
  Validacao minima:
```

## 7. Padroes de Codigo

Defina padroes locais do repositorio.

Exemplos:

- convencoes de nomes
- organizacao de arquivos
- quando usar BLL, service, endpoint, DTO
- como tratar erros
- como lidar com nullability

Template:

```md
## 7. Padroes de Codigo

- Padrao:
  Aplicacao:
  Excecoes permitidas:
```

## 8. Padroes de UI

Preencha se o projeto tiver frontend ou painel administrativo.

Exemplos:

- uso de componentes compartilhados
- padrao de dialogs
- navegacao e protecao de paginas
- convencoes de layout

Template:

```md
## 8. Padroes de UI

- Area:
  Regra:
  Arquivos de referencia:
```

## 9. Dados e Persistencia

Registre as regras relacionadas a banco e persistencia.

Exemplos:

- migrations
- naming de tabelas
- consultas performaticas
- padroes de transacao
- auditoria

Template:

```md
## 9. Dados e Persistencia

- Regra:
  Contexto:
  Arquivos criticos:
```

## 10. Observabilidade

Defina expectativas de logs, metricas e rastreabilidade.

Exemplos:

- o que deve ser logado
- o que nao deve ser logado
- correlacao de erros
- padrao de mensagens estruturadas

Template:

```md
## 10. Observabilidade

- Evento:
  Nivel minimo:
  Campos obrigatorios:
```

## 11. Testes e Validacao

Descreva o minimo esperado para aceitar uma mudanca.

Exemplos:

- testes obrigatorios por tipo de mudanca
- validacoes manuais minimas
- smoke tests de autenticacao
- regressao de contratos

Template:

```md
## 11. Testes e Validacao

- Tipo de mudanca:
  Testes esperados:
  Validacoes manuais:
```

## 12. Code Review

Padronize o que deve ser cobrado em revisao.

Exemplos:

- corretude
- regressao comportamental
- seguranca
- impacto em contratos
- clareza da implementacao

Template:

```md
## 12. Code Review

- Checklist:
  - 
  - 
  - 
```

## 13. Workflow de Mudanca

Defina um fluxo padrao para evolucoes no repositorio.

Exemplos:

- como propor mudancas maiores
- quando atualizar docs
- quando revisar contratos
- quando exigir migracao ou feature flag

Template:

```md
## 13. Workflow de Mudanca

1. Entender impacto.
2. Identificar contratos afetados.
3. Implementar mantendo limites arquiteturais.
4. Validar testes e smoke checks.
5. Atualizar documentacao aplicavel.
```

## 14. Decisoes Registradas

Use esta area para ADRs resumidos ou decisoes permanentes.

Template:

```md
## 14. Decisoes Registradas

- Decisao:
  Status:
  Data:
  Contexto:
  Consequencia:
```

## 15. Backlog de Regras

Se houver pontos importantes ainda nao formalizados, registre aqui.

Template:

```md
## 15. Backlog de Regras

- Tema pendente:
  Dono:
  Impacto:
```

## Convencao de Redacao

Para manter consistencia:

- use "MUST" para obrigatorio
- use "SHOULD" para recomendacao forte
- use "MAY" para opcional
- evite termos vagos como "preferencialmente" sem contexto
- sempre que possivel, referencie arquivos reais do repositorio

## Uso Com AGENTS.md

Sugestao de divisao de responsabilidade:

- `AGENTS.md`: contexto operacional do repositorio e alertas para execucao
- `constitution.md`: regras perenes, limites, criterios e principios

Quando uma regra virar padrao estavel do projeto, ela deve sair de comentarios dispersos e passar a constar aqui.
