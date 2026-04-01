# AGENTS.md

## Visão Geral

Esta solução é um `Blazor Web App` hospedado com `InteractiveAuto`, dividida em quatro projetos:

- `eTasks-server/eTasks-server/eTasks-server`: host web/server, endpoints, bootstrap, middleware, assets.
- `eTasks-server/eTasks-server/eTasks-server.Client`: UI, páginas, layouts e serviços do painel administrativo.
- `eTasks-server.Core`: regra de negócio, BLLs, acesso a dados e serviços internos.
- `eTasks-server.Models`: entidades, DTOs, constantes e exceções compartilhadas.

Há caminhos parecidos no repositório. O código real usado pela solução está dentro de `eTasks-server/eTasks-server/...`. Antes de editar, confirme o caminho.

## Estrutura Real

### Server

Arquivos centrais:

- `eTasks-server/eTasks-server/Program.cs`
- `eTasks-server/eTasks-server/Extensions/ServicesPayload.cs`
- `eTasks-server/eTasks-server/Extensions/MiddlewaresExtension.cs`
- `eTasks-server/eTasks-server/Extensions/MapResources.cs`
- `eTasks-server/eTasks-server/Components/App.razor`
- `eTasks-server/eTasks-server/Components/Routes.razor`
- `eTasks-server/eTasks-server/Endpoints/*`

Responsabilidades:

- bootstrap de DI
- autenticação e autorização
- mapeamento de endpoints `/api/v2/*`
- hospedagem do `Blazor Web App`

### Client

Arquivos centrais:

- `eTasks-server/eTasks-server.Client/Program.cs`
- `eTasks-server/eTasks-server.Client/Services/ClientServicesPayload.cs`
- `eTasks-server/eTasks-server.Client/Pages/*`
- `eTasks-server/eTasks-server.Client/Layout/*`
- `eTasks-server/eTasks-server.Client/Services/*`

Responsabilidades:

- UI administrativa
- páginas protegidas
- login/logout do painel
- consumo da API

### Core

Arquivos centrais:

- `eTasks-server.Core/BusinessLogicLayers/AuthBLL.cs`
- `eTasks-server.Core/BusinessLogicLayers/WebAuthBLL.cs`
- `eTasks-server.Core/BusinessLogicLayers/UserAdminBLL.cs`
- `eTasks-server.Core/BusinessLogicLayers/VersionBLL.cs`
- `eTasks-server.Core/Data/AppDbContext.cs`

Responsabilidades:

- JWT para clientes externos
- cookie auth para o painel web
- usuários, logs, refresh token e versão

### Models

Arquivos centrais:

- `eTasks-server.Models/Auth/AuthDTOs.cs`
- `eTasks-server.Models/Auth/WebLoginRequest.cs`
- `eTasks-server.Models/Users/*`
- `eTasks-server.Models/Utils/Constants.cs`
- `eTasks-server.Models/Exceptions/*`

## Bootstrapping

### Entrada do app

`eTasks-server/eTasks-server/Program.cs`:

1. `builder.RegisterServices()`
2. `app.RegisterMiddlewares()`
3. `await app.AddAPIEndpoints()`
4. `app.MapResourcesEndpoints()`

### Endpoints

`eTasks-server/eTasks-server/Endpoints/EndpointsEntry.cs` monta `/api/v2` e registra:

- version
- utils
- auth JWT
- web auth por cookie
- user admin

## Autenticação

O projeto hoje usa dois fluxos distintos. Isso é intencional.

### 1. JWT para clientes externos

Arquivos:

- `eTasks-server.Core/BusinessLogicLayers/AuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/AuthEndpoints.cs`

Rotas:

- `/api/v2/auth/login`
- `/api/v2/auth/refresh`

Uso:

- eTasks WASM
- cliente Delphi
- outros consumidores da API

Regra:

- não quebrar o contrato JWT ao ajustar o painel web

### 2. Cookie auth para o painel administrativo

Arquivos:

- `eTasks-server.Core/BusinessLogicLayers/WebAuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/WebAuthEndpoints.cs`
- `eTasks-server/eTasks-server/wwwroot/js/webAuth.js`
- `eTasks-server/eTasks-server.Client/Services/WebAuthService.cs`

Rotas:

- `POST /api/v2/web-auth/login`
- `POST /api/v2/web-auth/logout`

Fluxo:

- o login do painel valida credenciais e exige `Admin`
- o backend emite cookie via `SignInAsync`
- o navegador consome isso por `fetch(..., credentials: "include")`

### InteractiveAuto

O painel segue o padrão atual compatível com `InteractiveAuto`:

- server: `AddCascadingAuthenticationState()`
- server: `.AddAuthenticationStateSerialization(...)`
- client: `AddCascadingAuthenticationState()`
- client: `AddAuthenticationStateDeserialization()`

Arquivos:

- `eTasks-server/eTasks-server/Extensions/ServicesPayload.cs`
- `eTasks-server/eTasks-server.Client/Services/ClientServicesPayload.cs`

## Autorização

### UI

As páginas protegidas usam:

- `[Authorize]`
- `AuthorizeRouteView`

Arquivos principais:

- `eTasks-server/eTasks-server/Components/Routes.razor`
- `eTasks-server/eTasks-server.Client/Pages/Home.razor`
- `eTasks-server/eTasks-server.Client/Pages/Version.razor`
- `eTasks-server/eTasks-server.Client/Pages/ManageVersion.razor`
- `eTasks-server/eTasks-server.Client/Pages/ManageUsers.razor`

`Routes.razor` usa `RedirectToLogin` no bloco `NotAuthorized`.

### API

A política principal é `Admin`, registrada em `ServicesPayload.cs`.

Endpoints administrativos costumam usar:

- `.RequireAuthorization("Admin")`

## Serviços do Client

### BaseService

Arquivo:

- `eTasks-server/eTasks-server.Client/Services/BaseService.cs`

Regra atual:

- `GET` usa `HttpClient`
- `POST`, `PUT`, `PATCH` e `DELETE` usam `fetch` no navegador via `webAuth.send`

Motivo:

- chamadas mutáveis autenticadas do painel precisam enviar o cookie do navegador
- o tratamento foi centralizado para evitar repetição em novas features

Tratamento centralizado:

- `401`: redireciona para `/login?returnUrl=...`
- `403`: mostra diálogo de acesso negado

Ao criar novos serviços client-side que chamem a API, herde de `BaseService`.

### WebAuthService

Arquivo:

- `eTasks-server/eTasks-server.Client/Services/WebAuthService.cs`

Responsável apenas por:

- `LoginAsync`
- `LogoutAsync`

Ele usa JS interop para chamar `webAuth.login` e `webAuth.logout`.

### ReturnUrl

O retorno para a página anterior está distribuído entre:

- `eTasks-server/eTasks-server.Client/Components/RedirectToLogin.razor`
- `eTasks-server/eTasks-server.Client/Pages/Login.razor.cs`
- `eTasks-server/eTasks-server.Client/Services/BaseService.cs`

Regra:

- se a sessão expira ou a API responde `401`, o usuário vai para `/login?returnUrl=...`
- após login, volta para a rota relativa original, se válida

## Arquivos Críticos

### Configuração de DI e segurança

- `eTasks-server/eTasks-server/Extensions/ServicesPayload.cs`
- `eTasks-server/eTasks-server.Client/Services/ClientServicesPayload.cs`

### Login web

- `eTasks-server.Core/BusinessLogicLayers/WebAuthBLL.cs`
- `eTasks-server/eTasks-server/Endpoints/WebAuthEndpoints.cs`
- `eTasks-server/eTasks-server.Client/Pages/Login.razor.cs`
- `eTasks-server/eTasks-server.Client/Services/WebAuthService.cs`
- `eTasks-server/eTasks-server/wwwroot/js/webAuth.js`

### Consumo da API no painel

- `eTasks-server/eTasks-server.Client/Services/BaseService.cs`
- `eTasks-server/eTasks-server.Client/Services/UserAdminService.cs`
- `eTasks-server/eTasks-server.Client/Services/VersionService.cs`

## Convenções Importantes

### Base URL da API

`ServicesPayload.cs` contém `BuildApiBaseUrl(...)` para evitar duplicação de `/api/v2/`.

Se mexer em `appsettings` ou no `HttpClient`, valide isso primeiro. Esse problema já existiu.

### MudBlazor

O projeto usa `MudBlazor` no server e no client.

Tratamentos globais de UX devem preferir:

- `ISnackbar`
- `IDialogService`
- o fluxo centralizado do `BaseService`

### JS de autenticação

Se renomear funções em `wwwroot/js/webAuth.js`, sincronize:

- `WebAuthService.cs`
- `BaseService.cs`
- `Components/App.razor`

## Estado Atual da Segurança

Hoje o host usa um esquema híbrido em `ServicesPayload.cs`:

- cookie para o painel web
- bearer token para clientes externos

Ao mexer nisso, valide sempre:

1. JWT externo continua funcionando?
2. login web por cookie continua funcionando?
3. `[Authorize]` continua protegendo as páginas?
4. `returnUrl` continua preservado?
5. operações mutáveis da API continuam levando o cookie no navegador?

## Armadilhas Conhecidas

1. `InteractiveAuto` e JWT em `localStorage/sessionStorage` não são uma boa base para autenticar o painel.
   O caminho adotado aqui foi cookie auth para o Web App.

2. Há arquivos com encoding inconsistente no repositório.
   Se `apply_patch` falhar por UTF-8 inválido, pode ser necessário regravar o arquivo inteiro.

3. Existem classes legadas do fluxo JWT no client:
   - `Auth/TokenAuthenticationProvider.cs`
   - `Services/AuthService.cs`
   - `Services/TokenStorageService.cs`
   - interfaces relacionadas

   Elas não são mais a base da autenticação do painel web. Antes de remover, confirme se ainda existe alguma dependência residual.

4. O projeto usa `UseAntiforgery()`.
   Se novos `POST/PUT/PATCH/DELETE` do painel falharem, revise a interação entre antiforgery, cookie auth e `fetch`.

## Regra Prática Para Novas Features

Ao adicionar uma feature administrativa:

1. proteger a página com `[Authorize]` se necessário
2. criar serviço client herdando de `BaseService`
3. preferir endpoint com `.RequireAuthorization("Admin")`
4. evitar tratar `401/403` manualmente na página
5. se houver ação mutável, preservar o fluxo centralizado por `webAuth.send`

## Objetivo Arquitetural

O painel web e a API compartilham o mesmo backend, mas não o mesmo mecanismo principal de autenticação:

- painel web: cookie auth, compatível com `InteractiveAuto`
- clientes externos: JWT + refresh token

Esse é o ponto mais importante deste repositório. Se uma mudança quebrar essa separação, provavelmente quebrará o comportamento esperado do sistema.
