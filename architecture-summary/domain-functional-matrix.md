# Matriz Funcional do Dominio

Data de referencia: 2026-04-12
Projeto fonte: `eTasks-server.Models/Entities`
Contexto de mapeamento: `eTasks-server.Core/Data/AppDbContext.cs`

## Objetivo

Registrar, de forma operacional, a relacao entre cada recurso do produto, as entidades envolvidas e as principais regras de negocio esperadas no backend e no client.

## Matriz

| Recurso | Entidades principais | Regras importantes |
| --- | --- | --- |
| Configuracao global | `eTasksVersion` | Mantem um unico registro com a versao publicada do app. |
| Conta e perfil | `User`, `UserSettings` | `User` e o owner dos dados pessoais. `UserSettings` fica em relacao 1:1 e agora centraliza `Theme`, `Language`, `InitialScreen` e `EnableBonusSystem`. A tela inicial padrao e `Home`. |
| Autenticacao | `RefreshToken`, `PasswordResetCode`, `LoginLog` | JWT externo e cookie web continuam separados na arquitetura. Logs de login servem para auditoria. |
| Gamificacao | `BonusPointRule`, `UserBonusPoint`, `BonusAchievement`, `UserAchievement` | `BonusPointRule` centraliza os pontos padrao por origem. `UserBonusPoint` registra o historico de pontos. `SourceReferenceId` permite rastrear o recurso que originou a pontuacao. |
| Tarefas | `TaskItem`, `TaskRecurrence` | Tarefa pode ser unica ou recorrente. `TaskItem` representa a ocorrencia concreta. `TaskRecurrence` define repeticao diaria, semanal, mensal e afins. `GeneratedFromTaskId` liga ocorrencias geradas a uma tarefa base. `IsCompleted` e `CompletedAt` mantem o historico de conclusao. |
| Metas | `Goal` | Meta tem resumo, descricao, tipo, prioridade, status e `RewardPoints` opcional. O campo de pontos so precisa ser exigido pela regra de negocio quando a gamificacao estiver ativa em `UserSettings`. |
| Compras | `ShoppingList`, `ShoppingListItem` | Lista tem nome, lugar, tipo, totais e status finalizado. Item tem descricao, unidade, quantidade, valor unitario, total e status comprado. Se todos os itens estiverem comprados, a lista deve ser marcada como finalizada. |
| Anotacoes | `NoteItem` | Recurso simples de caderno pessoal. Persistir apenas assunto, texto, data de criacao e data de edicao. Nao gera pontos. |
| Leituras | `ReadingItem` | Registra titulo, autores, assunto, resumo, opiniao, rank, paginas, genero, formato, datas e status. Quando `CurrentPage` atingir `TotalPages`, a leitura deve poder ser marcada como concluida e gerar pontos conforme `BonusPointRule`. |
| Financas | `FinanceEntry` | Registra credito ou debito, forma de pagamento, valor, data e recorrencia opcional. Permite saldo mensal por competencia usando `OccursOn`, ou por caixa usando `PaidAt` e `IsPaid`. Saldo mensal positivo pode gerar pontos pela regra central. |

## Enums Estrategicos

- `BonusPointSource`: origem da pontuacao no sistema.
- `AppStartScreen`: tela inicial preferida do app.
- `TaskPriority`: prioridade visual e funcional para tarefas e metas.
- `RecurrenceType` e `WeekDays`: base da recorrencia em tarefas e financas.
- `GoalType` e `GoalStatus`: classificacao e estado das metas.
- `ShoppingListType` e `ShoppingItemUnit`: tipificacao das compras.
- `ReadingStatus` e `ReadingFormat`: estado e formato das leituras.
- `FinanceEntryType` e `FinancePaymentMethod`: classificacao dos lancamentos financeiros.

## Regras Transversais

- Quase todo o dominio pessoal pertence a um `User`.
- Pontos nao devem ficar hardcoded nas entidades de negocio quando houver regra central equivalente.
- Recorrencia exige geracao de ocorrencias concretas para a agenda do usuario.
- Campos de totalizacao, como `ShoppingList.TotalAmount` e `ShoppingListItem.TotalAmount`, devem ser mantidos sincronizados pela camada de negocio.
- Regras condicionais de obrigatoriedade, como `Goal.RewardPoints` depender de gamificacao ativa, devem ficar na validacao de negocio, nao apenas no banco.
