# AGENTS.md

## Objetivo

Este arquivo orienta agentes que trabalham neste repositorio. O foco e manter as instrucoes alinhadas com a estrutura real da solucao atual, evitar edicoes no caminho errado e preservar as decisoes arquiteturais ja adotadas.

## Estrutura Atual da Solucao

Hoje a solucao carregada por `eTasks-server.slnx` possui **3 projetos**:

- `eTasks-server/eTasks-server/eTasks-server.csproj`
- `eTasks-server.Core/eTasks-server.Core.csproj`
- `eTasks-server.Models/eTasks-server.Models.csproj`

Importante:

- O painel administrativo **nao esta em um projeto separado** na solucao atual.
- O codigo de UI do painel fica dentro do projeto web, em `eTasks-server/eTasks-server/Client`.
- Existe uma pasta `eTasks-server.Client` na raiz do repositorio, mas ela **nao faz parte da solucao atual**. Antes de editar qualquer arquivo nela, confirme se a mudanca realmente deve atingir codigo fora da solucao ativa.

## Mapa do Repositorio

### 1. Web Host / Server

Caminho real:

- `eTasks-server/eTasks-server`

Arquivos centrais:

- `eTasks-server/eTasks-server/Program.cs`
- `eTasks-server/eTasks-server/Extensions/ServicesPayload.cs`
- `eTasks-server/eTasks-server/Extensions/MiddlewaresExtension.cs`
- `eTasks-server/eTasks-server/Extensions/MapResources.cs`
- `eTasks-server/eTasks-server/Components/App.razor`
- `eTasks-server/eTasks-server/Components/Routes.razor`
- `eTasks-server/eTasks-server/Endpoints/*`
- `eTasks-server/eTasks-server/wwwroot/*`

Responsabilidades:

- bootstrap da aplicacao
- DI
- autenticacao e autorizacao
- middlewares
- endpoints `/api/v2/*`
- hospedagem do painel Blazor
- assets estaticos

### 2. Client do Painel Administrativo

Caminho real:

- `eTasks-server/eTasks-server/Client`

Arquivos centrais:

- `eTasks-server/eTasks-server/Client/Pages/*`
- `eTasks-server/eTasks-server/Client/Layout/*`
- `eTasks-server/eTasks-server/Client/Components/*`
- `eTasks-server/eTasks-server/Client/Services/*`

Responsabilidades:

- UI administrativa
- paginas autenticadas
- login web
- dialogs e layout
- consumo de servicos internos registrados no host

### 3. Core

Caminho real:

- `eTasks-server.Core`

Arquivos centrais:

- `eTasks-server.Core/BusinessLogicLayers/AuthBLL.cs`
- `eTasks-server.Core/BusinessLogicLayers/WebAuthBLL.cs`
- `eTasks-server.Core/BusinessLogicLayers/UserAdminBLL.cs`
- `eTasks-server.Core/BusinessLayers/VersionBLL.cs`
- `eTasks-server.Core/Data/AppDbContext.cs`
- `eTasks-server.Core/Services/*`

Responsabilidades:

- regras de negocio
- autenticacao JWT para clientes externos
- autenticacao por cookie para o painel web
- acesso a dados via EF Core
- servicos internos como e-mail e perfil

### 4. Models

Caminho real:

- `eTasks-server.Models`

Responsabilidades:

- entidades
- DTOs
- constantes
- excecoes
- contratos compartilhados entre host e core

## Bootstrap da Aplicacao

Entrada principal:

- `eTasks-server/eTasks-server/Program.cs`

Fluxo atual:

1. `builder.RegisterServices()`
2. `var app = builder.Build()`
3. `app.RegisterMiddlewares()`
4. `await app.AddAPIEndpoints()`
5. `app.MapResourcesEndpoints()`
6. `app.Run()`

## Registro de Servicos

Arquivo principal:

- `eTasks-server/eTasks-server/Extensions/ServicesPayload.cs`

Responsabilidades atuais desse arquivo:

- banco MySQL
- Razor Components com `AddInteractiveServerComponents()`
- CORS
- MudBlazor
- health checks
- OpenAPI
- exception handler global
- autenticacao hibrida
- politicas de autorizacao
- registro dos servicos usados pelo painel

Observacoes importantes:

- O estado atual usa `InteractiveServer`, nao o fluxo antigo descrito em versoes anteriores deste arquivo.
- Os servicos do painel (`VersionService`, `UserAdminService`, `UserLogsDrawerService`) sao registrados no host e consumidos pela UI do proprio app.
- Hoje nao existe `ClientServicesPayload.cs` na solucao ativa.

## Endpoints

Arquivo central:

- `eTasks-server/eTasks-server/Endpoints/EndpointsEntry.cs`

Prefixo atual:

- `/api/v2`

Grupos registrados hoje:

- `version`
- `utils`
- `auth`
- `web-auth`
- `users`
- `usuarios`

Arquivos relevantes:

- `eTasks-server/eTasks-server/Endpoints/AuthEndpoints.cs`
- `eTasks-server/eTasks-server/Endpoints/WebAuthEndpoints.cs`
- `eTasks-server/eTasks-server/Endpoints/UserAdminEndpoints.cs`
- `eTasks-server/eTasks-server/Endpoints/UsuariosEndpoints.cs`
- `eTasks-server/eTasks-server/Endpoints/VersionEndpoint.cs`
- `eTasks-server/eTasks-server/Endpoints/UtilsEndpoint.cs`

## Autenticacao

O projeto continua com dois fluxos distintos. Essa separacao deve ser preservada.

### 1. JWT para clientes externos

Arquivos principais:

- `eTasks-server.Core/BusinessLogicLayers/AuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/AuthEndpoints.cs`

Uso:

- consumidores externos da API
- apps clientes que usam bearer token
- refresh token com suporte a cookie HttpOnly em cenarios web especificos

Regras:

- nao quebrar contratos de login, refresh, logout, registro e confirmacao
- manter compatibilidade com `Authorization: Bearer`

### 2. Cookie auth para o painel administrativo

Arquivos principais:

- `eTasks-server.Core/BusinessLogicLayers/WebAuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/WebAuthEndpoints.cs`
- `eTasks-server/eTasks-server/Client/Pages/Login.razor`
- `eTasks-server/eTasks-server/Components/Routes.razor`

Fluxo atual:

- a tela de login faz `POST` tradicional para `/api/v2/web-auth/login`
- o endpoint recebe `WebLoginRequest` via `FromForm`
- o backend autentica o usuario admin
- o backend emite cookie com `SignInAsync`
- o endpoint responde com redirect local seguro para `returnUrl`
- o logout atual e feito via `GET /api/v2/web-auth/logout`

Nao assuma mais:

- existencia de `webAuth.js`
- `fetch(..., credentials: "include")` como base do painel
- `WebAuthService`
- `BaseService`

Esses itens faziam parte de uma organizacao anterior e nao representam o estado atual da solucao ativa.

## Autorizacao

### UI

Arquivos principais:

- `eTasks-server/eTasks-server/Components/Routes.razor`
- `eTasks-server/eTasks-server/Client/Pages/Home.razor`
- `eTasks-server/eTasks-server/Client/Pages/Version.razor`
- `eTasks-server/eTasks-server/Client/Pages/ManageVersion.razor`
- `eTasks-server/eTasks-server/Client/Pages/ManageUsers.razor`

Padrao atual:

- `AuthorizeRouteView`
- paginas protegidas com `[Authorize]`
- `RedirectToLogin` no fluxo de `NotAuthorized`

### API

Politicas registradas em `ServicesPayload.cs`:

- `Admin`
- `WebAdmin`

Regra pratica:

- endpoints administrativos web devem preferir `.RequireAuthorization("WebAdmin")`
- o uso de `WebAdmin` deixa explicito que a rota deve usar o esquema de cookie para o painel

## Middlewares e Pipeline

Arquivo principal:

- `eTasks-server/eTasks-server/Extensions/MiddlewaresExtension.cs`

Pontos atuais do pipeline:

- `UseCors(...)`
- `UseExceptionHandler()`
- `MapOpenApi()`
- `MapScalarApiReference(...)`
- `UseStatusCodePages(...)` para respostas JSON em rotas `/api`
- `UseAuthentication()`
- `UseAuthorization()`
- `UseHttpsRedirection()`
- `UseAntiforgery()`

Atenções:

- o projeto usa `Scalar` para referencia da API
- `UseAntiforgery()` ja esta ativo
- mudanças em rotas mutaveis do painel devem considerar antiforgery e auth por cookie

## Servicos do Painel

Arquivos atuais:

- `eTasks-server/eTasks-server/Client/Services/UserAdminService.cs`
- `eTasks-server/eTasks-server/Client/Services/VersionService.cs`
- `eTasks-server/eTasks-server/Client/Services/UserLogsDrawerService.cs`

Observacao importante:

- esses servicos nao sao wrappers HTTP genericos como na organizacao anterior
- hoje eles operam dentro do app, consumindo BLLs e contexto registrados por DI

Se uma nova feature administrativa for implementada, confirme primeiro qual modelo faz sentido:

- servico interno via DI, quando a UI roda no mesmo host e a chamada e local
- endpoint HTTP protegido, quando a funcionalidade tambem precisa ficar disponivel por API

Nao reintroduza um padrao antigo por inercia sem validar a necessidade arquitetural.

## Arquivos Criticos

### Seguranca e DI

- `eTasks-server/eTasks-server/Extensions/ServicesPayload.cs`
- `eTasks-server/eTasks-server/Extensions/MiddlewaresExtension.cs`

### Login web

- `eTasks-server.Core/BusinessLogicLayers/WebAuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/WebAuthEndpoints.cs`
- `eTasks-server/eTasks-server/Client/Pages/Login.razor`
- `eTasks-server/eTasks-server/Components/Routes.razor`

### JWT externo

- `eTasks-server.Core/BusinessLogicLayers/AuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/AuthEndpoints.cs`

### Gerenciamento administrativo

- `eTasks-server.Core/BusinessLogicLayers/UserAdminBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/UserAdminEndpoints.cs`
- `eTasks-server/eTasks-server/Client/Pages/ManageUsers.razor`
- `eTasks-server/eTasks-server/Client/Services/UserAdminService.cs`

### Versao

- `eTasks-server.Core/BusinessLayers/VersionBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/VersionEndpoint.cs`
- `eTasks-server/eTasks-server/Client/Pages/Version.razor`
- `eTasks-server/eTasks-server/Client/Pages/ManageVersion.razor`
- `eTasks-server/eTasks-server/Client/Services/VersionService.cs`

## Regras Praticas Para Agentes

Antes de editar:

1. confirme se o arquivo pertence a um dos 3 projetos da solucao atual
2. desconfie de caminhos parecidos fora da solucao ativa
3. valide se a mudanca afeta JWT externo, cookie auth web, ou ambos

Ao adicionar feature administrativa:

1. proteger a pagina com `[Authorize]` quando aplicavel
2. avaliar se a chamada deve ser local via DI ou exposta como endpoint HTTP
3. se houver endpoint administrativo web, preferir `.RequireAuthorization("WebAdmin")`
4. manter `returnUrl` seguro e relativo no fluxo de login/logout
5. considerar antiforgery em formularios e operacoes mutaveis

Ao alterar autenticacao:

1. JWT externo continua funcionando?
2. login do painel por cookie continua funcionando?
3. `AuthorizeRouteView` continua redirecionando corretamente?
4. `returnUrl` continua validado como relativo?
5. endpoints administrativos continuam protegidos por `WebAdmin`?

## Armadilhas Conhecidas

1. Ha caminhos antigos e parecidos no repositorio. Edite somente o caminho realmente usado pela solucao.
2. O texto antigo deste arquivo citava um projeto client separado e servicos que nao existem mais na solucao ativa.
3. Existem arquivos com encoding inconsistente no repositorio. Se um patch falhar por encoding, pode ser necessario regravar o arquivo inteiro.
4. Alteracoes em autenticacao costumam ter impacto cruzado entre painel web e API externa. Teste os dois fluxos.
5. `MapScalarApiReference(...)` esta protegido. Mudancas de auth podem afetar acesso a documentacao.

## Objetivo Arquitetural

O sistema compartilha backend, mas mantem responsabilidades separadas:

- painel administrativo web: auth por cookie
- clientes externos: auth por JWT e refresh token
- regras de negocio: centralizadas em `Core`
- contratos e entidades: centralizados em `Models`

Qualquer mudanca que misture indevidamente esses fluxos tende a introduzir regressao.
