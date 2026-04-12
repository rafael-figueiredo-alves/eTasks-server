# Resumo Atual das Entidades do Banco

Data de referencia: 2026-04-08
Projeto fonte: `eTasks-server.Models/Entities`
Contexto de mapeamento: `eTasks-server.Core/Data/AppDbContext.cs`

## Visao Geral

Hoje o modelo persistido do sistema possui 18 entidades mapeadas no `AppDbContext`:

1. `eTasksVersion`
2. `User`
3. `RefreshToken`
4. `PasswordResetCode`
5. `LoginLog`
6. `UserSettings`
7. `UserBonusPoint`
8. `BonusAchievement`
9. `UserAchievement`
10. `BonusPointRule`
11. `TaskItem`
12. `TaskRecurrence`
13. `Goal`
14. `ShoppingList`
15. `ShoppingListItem`
16. `NoteItem`
17. `ReadingItem`
18. `FinanceEntry`

O dominio persistido continua centrado em `User`, mas agora esta organizado em oito blocos principais:

- configuracao global
- identidade, autenticacao e perfil
- gamificacao
- tarefas
- metas
- compras
- anotacoes e leituras
- financas

## Leitura Arquitetural Rapida

### 1. Nucleo de identidade

A entidade central continua sendo `User`.
Ela concentra identidade, credenciais, estado de conta e o ownership dos registros pessoais do aplicativo.

### 2. Seguranca e autenticacao

O conjunto `RefreshToken`, `PasswordResetCode` e `LoginLog` representa o bloco operacional de autenticacao:

- `RefreshToken`: sessao longa para JWT
- `PasswordResetCode`: recuperacao de senha com expiracao e uso unico
- `LoginLog`: trilha de auditoria de tentativas de login

### 3. Perfil do usuario

`UserSettings` isola preferencias em relacao 1:1 com `User`.

### 4. Gamificacao

O bloco de pontos agora possui tres camadas:

- `BonusPointRule`: catalogo central de regras de pontuacao por origem
- `UserBonusPoint`: historico de lancamentos de pontos por usuario
- `BonusAchievement` + `UserAchievement`: catalogo e historico de conquistas

### 5. Tarefas

O bloco de produtividade foi modelado com:

- `TaskItem`: tarefa concreta visivel para o usuario
- `TaskRecurrence`: configuracao de recorrencia 1:1 da tarefa base

O modelo tambem permite autorrelacao em `TaskItem` via `GeneratedFromTaskId` para representar tarefas futuras geradas a partir de uma tarefa raiz.

### 6. Metas

`Goal` representa objetivos simples do usuario com resumo, descricao, tipo, prioridade e status.

### 7. Compras

O bloco de compras foi modelado com:

- `ShoppingList`: lista principal
- `ShoppingListItem`: itens da lista

As listas agora carregam tipo, lugar, totais e status finalizada.
Os itens carregam descricao, unidade, quantidade, valor unitario e total do item.

### 8. Conhecimento pessoal

Dois recursos foram separados:

- `NoteItem`: anotacoes livres sem pontuacao
- `ReadingItem`: leituras com progresso, avaliacao e recompensa opcional

### 9. Financas

`FinanceEntry` representa entradas e saidas financeiras, recorrentes ou nao, dentro do mesmo agregado.

## Entidades

### `eTasksVersion`

Tabela: `version`
Chave primaria: `Id`
Natureza: configuracao singleton

Campos principais:

- `Id` fixo em `1`
- `AppVersion`
- `DisplayVersion`
- `URL_APK`
- `URL_Win`

Observacoes:

- o sistema assume um unico registro
- esta entidade funciona mais como configuracao persistida do que como agregado de negocio

### `User`

Tabela: `users`
Chave primaria: `Uid`
Indice unico: `Email`
Natureza: agregado principal do dominio

Campos principais:

- identificacao: `Uid`, `Name`, `Email`
- autenticacao: `PasswordHash`
- perfil: `PhotoPath`
- controle de acesso: `IsConfirmed`, `IsAdmin`, `IsBlocked`
- ciclo de vida: `CreatedAt`, `LastAccessAt`, `IsDeleted`, `DeletedAt`

Relacionamentos:

- 1:N com `RefreshToken`
- 1:N com `PasswordResetCode`
- 1:N com `LoginLog`
- 1:1 com `UserSettings`
- 1:N com `UserBonusPoint`
- 1:N com `UserAchievement`
- 1:N com `TaskItem`
- 1:N com `Goal`
- 1:N com `ShoppingList`
- 1:N com `NoteItem`
- 1:N com `ReadingItem`
- 1:N com `FinanceEntry`

Observacoes arquiteturais:

- `User` continua sendo o principal centro gravitacional do banco
- ele agora funciona como dono dos registros pessoais dos modulos principais do app

### `RefreshToken`

Tabela: `refresh_tokens`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: sessao renovavel de autenticacao

Campos principais:

- `Token`
- `UserAgent`
- `ExpiresAt`
- `IsRevoked`
- `CreatedAt`

### `PasswordResetCode`

Tabela: `password_reset_codes`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: token de recuperacao de senha de curta duracao

Campos principais:

- `Code`
- `ExpiresAt`
- `IsUsed`
- `CreatedAt`

### `LoginLog`

Tabela: `login_logs`
Chave primaria: `Id`
Chave de referencia opcional: `UserUid -> User.Uid`
Natureza: auditoria de login

Campos principais:

- `Status`
- `IpAddress`
- `UserAgent`
- `CreatedAt`

### `UserSettings`

Tabela: `user_settings`
Chave primaria: `Id`
Chave estrangeira unica: `UserUid -> User.Uid`
Natureza: extensao 1:1 de preferencias do usuario

Campos principais:

- `Theme`
- `Language`
- `UseCamera`
- `EnableBonusSystem`
- `CreatedAt`
- `UpdatedAt`

### `BonusPointRule`

Tabela: `bonus_point_rules`
Chave primaria: `Id`
Indice unico: `Source`
Natureza: catalogo central de regras de pontuacao

Campos principais:

- `Source`
- `Name`
- `Description`
- `DefaultPoints`
- `AllowCustomPoints`
- `IsActive`

Observacoes:

- foi introduzida para centralizar os pontos padrao por tipo de recompensa
- prepara o sistema para mudar pontuacao sem espalhar valores fixos pela aplicacao

### `UserBonusPoint`

Tabela: `user_bonus_points`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: lancamento de pontos

Campos principais:

- `Points`
- `Source`
- `Description`
- `SourceReferenceId`
- `CreatedAt`

Observacoes:

- `Source` agora e enum persistido como inteiro
- `SourceReferenceId` permite rastrear o recurso que originou a pontuacao
- o saldo total continua derivado pela soma dos lancamentos

### `BonusAchievement`

Tabela: `bonus_achievements`
Chave primaria: `Id`
Indice unico: `Code`
Natureza: catalogo mestre de conquistas

Campos principais:

- `Code`
- `Name`
- `Description`
- `PointsRequired`
- `DisplayType`
- `IsActive`

### `UserAchievement`

Tabela: `user_achievements`
Chave primaria: `Id`
Chaves estrangeiras:

- `UserUid -> User.Uid`
- `BonusAchievementId -> BonusAchievement.Id`

Indice unico composto:

- `(UserUid, BonusAchievementId)`

Natureza: registro de conquista adquirida pelo usuario

Campos principais:

- `PointsAtAchievement`
- `AchievedAt`

### `TaskItem`

Tabela: `task_items`
Chave primaria: `Id`
Chaves estrangeiras:

- `UserUid -> User.Uid`
- `GeneratedFromTaskId -> TaskItem.Id` opcional

Natureza: tarefa concreta do usuario

Campos principais:

- `Title`
- `Description`
- `Priority`
- `ScheduledFor`
- `DueAt`
- `IsCompleted`
- `CompletedAt`
- `IsArchived`
- `CreatedAt`

Observacoes:

- suporta tarefas simples e tarefas geradas por recorrencia
- a autorrelacao permite encadear tarefas filhas a partir de uma tarefa de origem

### `TaskRecurrence`

Tabela: `task_recurrences`
Chave primaria: `Id`
Chave estrangeira unica: `TaskItemId -> TaskItem.Id`
Natureza: configuracao de recorrencia da tarefa base

Campos principais:

- `RecurrenceType`
- `Interval`
- `WeekDays`
- `DayOfMonth`
- `MonthOfYear`
- `StartsOn`
- `EndsOn`
- `LastGeneratedAt`
- `IsActive`

Observacoes:

- existe em relacao 1:1 com a tarefa que serve como template

### `Goal`

Tabela: `goals`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: meta ou objetivo do usuario

Campos principais:

- `Summary`
- `Description`
- `Type`
- `Priority`
- `RewardPoints`
- `Status`
- `CreatedAt`
- `UpdatedAt`

### `ShoppingList`

Tabela: `shopping_lists`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: lista de compras

Campos principais:

- `Name`
- `Place`
- `Type`
- `TotalItems`
- `TotalAmount`
- `IsFinalized`

### `ShoppingListItem`

Tabela: `shopping_list_items`
Chave primaria: `Id`
Chave estrangeira: `ShoppingListId -> ShoppingList.Id`
Natureza: item de compra

Campos principais:

- `Description`
- `Unit`
- `Quantity`
- `UnitPrice`
- `TotalAmount`
- `IsCompleted`

### `NoteItem`

Tabela: `notes`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: anotacao livre do usuario

Campos principais:

- `Subject`
- `Content`
- `CreatedAt`
- `UpdatedAt`

Observacoes:

- nao participa do sistema de pontuacao
- foi mantida propositalmente simples, como um caderno pessoal do usuario

### `ReadingItem`

Tabela: `reading_items`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: registro de leitura

Campos principais:

- `Title`
- `Author`
- `Description`
- `TotalPages`
- `CurrentPage`
- `Rating`
- `RewardPoints`
- `Status`
- `StartedAt`
- `FinishedAt`

### `FinanceEntry`

Tabela: `finance_entries`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: lancamento financeiro

Campos principais:

- `Title`
- `Description`
- `Category`
- `Counterparty`
- `EntryType`
- `PaymentMethod`
- `Amount`
- `OccursOn`
- `IsPaid`
- `PaidAt`
- `IsRecurring`
- `RecurrenceType`
- `RecurrenceInterval`
- `WeekDays`
- `DayOfMonth`
- `RecurrenceEndsOn`

Observacoes:

- concentra entradas e saidas no mesmo modelo
- tambem suporta recorrencia sem exigir tabela separada
- permite calcular saldo mensal por competencia (`OccursOn`) ou por caixa (`PaidAt` + `IsPaid`)

## Relacionamentos Consolidados

- `User` 1:N `RefreshToken`
- `User` 1:N `PasswordResetCode`
- `User` 1:N `LoginLog`
- `User` 1:1 `UserSettings`
- `User` 1:N `UserBonusPoint`
- `User` 1:N `UserAchievement`
- `BonusAchievement` 1:N `UserAchievement`
- `User` 1:N `TaskItem`
- `TaskItem` 1:1 `TaskRecurrence`
- `TaskItem` 1:N `TaskItem` via `GeneratedFromTaskId`
- `User` 1:N `Goal`
- `User` 1:N `ShoppingList`
- `ShoppingList` 1:N `ShoppingListItem`
- `User` 1:N `NoteItem`
- `User` 1:N `ReadingItem`
- `User` 1:N `FinanceEntry`
- `eTasksVersion` e `BonusPointRule` nao dependem de outras entidades

## Pontos Estruturais Relevantes para Decisao

### 1. `User` segue como dono de quase todo o dominio pessoal

Isso simplifica leitura e filtros por usuario, mas reforca o papel central de `User` em varios subdominios.

### 2. Gamificacao ficou mais preparada para evolucao

Com `BonusPointRule`, `UserBonusPoint`, `BonusAchievement` e `UserAchievement`, o modulo de pontos agora tem uma fronteira mais clara.

### 3. Tarefas e financas usam estrategias diferentes para recorrencia

- tarefas: recorrencia em entidade separada (`TaskRecurrence`)
- financas: recorrencia embutida no proprio `FinanceEntry`

Essa diferenca e aceitavel no estado atual, mas pode ser revisitada se surgir necessidade de padrao unico.

### 4. O dominio agora cobre quase todo o produto final

As novas entidades registram a base persistida dos recursos centrais do app:

- tarefas
- metas
- compras
- anotacoes
- leituras
- financas
- gamificacao

### 5. O modelo foi desenhado priorizando ownership individual

Quase todos os recursos sao pessoais e dependem diretamente de `UserUid`.
Se no futuro surgirem recursos compartilhados, o modelo provavelmente precisara de entidades associativas especificas.

## Resumo Executivo

Se eu resumisse o modelo atual em uma frase:

> O banco agora representa um dominio pessoal de produtividade e vida cotidiana, centrado em `User`, cobrindo autenticacao, configuracoes, gamificacao, tarefas, metas, compras, anotacoes, leituras e financas.

## Arquivos de Referencia

- `eTasks-server.Models/Entities/Version/eTasksVersion.cs`
- `eTasks-server.Models/Entities/Users/User.cs`
- `eTasks-server.Models/Entities/Users/RefreshToken.cs`
- `eTasks-server.Models/Entities/Users/PasswordResetCode.cs`
- `eTasks-server.Models/Entities/Users/LoginLog.cs`
- `eTasks-server.Models/Entities/Users/UserSettings.cs`
- `eTasks-server.Models/Entities/Gamification/BonusPointRule.cs`
- `eTasks-server.Models/Entities/Users/UserBonusPoint.cs`
- `eTasks-server.Models/Entities/Users/BonusAchievement.cs`
- `eTasks-server.Models/Entities/Users/UserAchievement.cs`
- `eTasks-server.Models/Entities/Productivity/TaskItem.cs`
- `eTasks-server.Models/Entities/Productivity/TaskRecurrence.cs`
- `eTasks-server.Models/Entities/Goals/Goal.cs`
- `eTasks-server.Models/Entities/Shopping/ShoppingList.cs`
- `eTasks-server.Models/Entities/Shopping/ShoppingListItem.cs`
- `eTasks-server.Models/Entities/Notes/NoteItem.cs`
- `eTasks-server.Models/Entities/Readings/ReadingItem.cs`
- `eTasks-server.Models/Entities/Finances/FinanceEntry.cs`
- `eTasks-server.Core/Data/AppDbContext.cs`
