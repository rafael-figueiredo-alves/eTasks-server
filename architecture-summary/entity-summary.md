# Resumo Atual das Entidades do Banco

Data de referencia: 2026-04-05
Projeto fonte: `eTasks-server.Models/Entities`
Contexto de mapeamento: `eTasks-server.Core/Data/AppDbContext.cs`

## Visao Geral

Hoje o modelo persistido do sistema possui 9 entidades mapeadas no `AppDbContext`:

1. `eTasksVersion`
2. `User`
3. `RefreshToken`
4. `PasswordResetCode`
5. `LoginLog`
6. `UserSettings`
7. `UserBonusPoint`
8. `BonusAchievement`
9. `UserAchievement`

O dominio persistido e fortemente centrado em `User`.
Existem tres blocos principais:

- autenticacao e ciclo de conta
- configuracao/perfil do usuario
- sistema de pontuacao e conquistas

## Leitura Arquitetural Rapida

### 1. Nucleo de identidade

A entidade central e `User`. Quase todo o restante depende dela direta ou indiretamente.

Ela concentra:

- identidade e credenciais
- flags de acesso (`IsAdmin`, `IsBlocked`, `IsConfirmed`, `IsDeleted`)
- metadados de ciclo de vida (`CreatedAt`, `LastAccessAt`, `DeletedAt`)
- relacionamentos com tokens, codigos de reset, configuracoes e bonus

### 2. Seguranca e autenticacao

O conjunto `RefreshToken`, `PasswordResetCode` e `LoginLog` representa o bloco de autenticacao operacional:

- `RefreshToken`: sessao longa para JWT
- `PasswordResetCode`: recuperacao de senha com expiracao e uso unico
- `LoginLog`: trilha de auditoria de tentativas de login

### 3. Perfil e preferencias

`UserSettings` isola preferencias do usuario em relacao 1:1 com `User`.

### 4. Gamificacao / bonus

O bloco de bonus foi modelado com catalogo + vinculo historico:

- `BonusAchievement`: catalogo mestre de conquistas
- `UserBonusPoint`: lancamentos de pontos por usuario
- `UserAchievement`: snapshot de conquista alcancada por um usuario

### 5. Configuracao global da aplicacao

`eTasksVersion` e uma entidade singleton usada como configuracao persistida da versao atual do app.

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
- esta entidade funciona mais como configuracao persistida do que como agregado de dominio

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
- 1:1 com `UserSettings`
- 1:N com `UserBonusPoint`
- 1:N com `UserAchievement`
- possui navegacao em `LoginLog`, mas esse lado nao esta exposto como colecao na entidade `User`

Observacoes arquiteturais:

- `User` concentra muitas responsabilidades
- hoje ele mistura autenticacao, autorizacao, estado de conta, perfil e gancho para gamificacao
- para decisoes futuras, vale avaliar se o agregado esta ficando largo demais

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

Observacoes:

- pertence ao usuario
- o estado da sessao fica modelado na propria entidade
- e usada tanto para renovacao quanto para revogacao forcada

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

Observacoes:

- modela expiracao e uso unico
- e uma boa entidade transacional de seguranca

### `LoginLog`

Tabela: `login_logs`
Chave primaria: `Id`
Chave de referencia opcional: `UserUid -> User.Uid`
Natureza: auditoria de login

Campos principais:

- `Status` (`Success`, `Failed`, `Blocked`)
- `IpAddress`
- `UserAgent`
- `CreatedAt`

Observacoes:

- aceita `UserUid` nulo, o que permite registrar tentativas sem usuario identificado
- o relacionamento com `User` nao esta explicitamente configurado no `OnModelCreating`, embora a navegacao exista
- e uma entidade importante para observabilidade e seguranca

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

Observacoes:

- separa configuracao de preferencia do agregado principal
- a unicidade de `UserUid` reforca o modelo 1:1

### `UserBonusPoint`

Tabela: `user_bonus_points`
Chave primaria: `Id`
Chave estrangeira: `UserUid -> User.Uid`
Natureza: lancamento de pontos

Campos principais:

- `Points`
- `Source`
- `Description`
- `CreatedAt`

Observacoes:

- o saldo total nao e armazenado no usuario; ele e derivado pela soma dos lancamentos
- esse modelo favorece historico e auditoria, mas pode exigir otimizacao futura em cenarios de alto volume

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
- `IsActive`
- `CreatedAt`

Observacoes:

- representa definicoes estaveis de conquista
- funciona como tabela de referencia para `UserAchievement`

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

Observacoes:

- guarda o momento em que a conquista foi obtida
- `PointsAtAchievement` preserva o snapshot do contexto, o que e bom para auditoria historica
- o delete da relacao para `BonusAchievement` esta como `Restrict`

## Relacionamentos Consolidados

- `User` 1:N `RefreshToken`
- `User` 1:N `PasswordResetCode`
- `User` 1:1 `UserSettings`
- `User` 1:N `UserBonusPoint`
- `User` 1:N `UserAchievement`
- `BonusAchievement` 1:N `UserAchievement`
- `LoginLog` referencia `User` de forma opcional
- `eTasksVersion` nao depende de outras entidades

## Pontos Estruturais Relevantes para Decisao

### 1. `User` e um agregado muito central

Hoje o sistema orbita em torno de `User`. Isso simplifica o inicio do projeto, mas aumenta acoplamento entre:

- autenticacao
- administracao
- perfil
- preferencias
- bonus/gamificacao

Se o projeto crescer, pode valer separar melhor os subdominios sem necessariamente quebrar a tabela imediatamente.

### 2. O modulo de autenticacao ja tem persistencia propria razoavel

`RefreshToken`, `PasswordResetCode` e `LoginLog` ja formam um bloco coerente.
Isso favorece uma futura modularizacao de seguranca sem reescrever o dominio inteiro.

### 3. O modulo de bonus esta relativamente bem desacoplado

A combinacao `BonusAchievement` + `UserBonusPoint` + `UserAchievement` ja cria uma fronteira util para evolucao independente.
Se houver crescimento desse dominio, ele parece um bom candidato a modulo proprio.

### 4. `eTasksVersion` nao se comporta como entidade de negocio comum

Ela parece mais uma configuracao singleton persistida.
Estruturalmente, poderia no futuro migrar para:

- tabela/config separada de settings globais
- provider de configuracao administravel
- aggregate de release/versionamento, se esse dominio crescer

### 5. Falta explicitar alguns relacionamentos de auditoria

`LoginLog` possui `UserUid` e navegacao para `User`, mas essa relacao nao esta detalhada no `OnModelCreating`.
Para clareza arquitetural, pode ser interessante explicitar isso no mapeamento.

## Resumo Executivo

Se eu resumisse o modelo atual em uma frase:

> O banco hoje e um dominio centrado em `User`, com extensoes para autenticacao operacional, preferencias do usuario e gamificacao, alem de uma configuracao singleton de versao da aplicacao.

Em termos de risco arquitetural, o principal ponto de atencao nao esta na quantidade de entidades, mas na concentracao de responsabilidades em `User`.

## Arquivos de Referencia

- `eTasks-server.Models/Entities/Version/eTasksVersion.cs`
- `eTasks-server.Models/Entities/Users/User.cs`
- `eTasks-server.Models/Entities/Users/RefreshToken.cs`
- `eTasks-server.Models/Entities/Users/PasswordResetCode.cs`
- `eTasks-server.Models/Entities/Users/LoginLog.cs`
- `eTasks-server.Models/Entities/Users/UserSettings.cs`
- `eTasks-server.Models/Entities/Users/UserBonusPoint.cs`
- `eTasks-server.Models/Entities/Users/BonusAchievement.cs`
- `eTasks-server.Models/Entities/Users/UserAchievement.cs`
- `eTasks-server.Core/Data/AppDbContext.cs`
