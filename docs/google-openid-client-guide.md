# Login com Google OpenID Connect

Este fluxo serve para clientes externos do eTasks: PWA/Blazor WASM (`web`), Delphi Windows (`windows`) e Delphi Android (`android`). O servidor eTasks faz o callback do Google, cria ou vincula o usuario e emite o mesmo `LoginResponse` usado em `/api/v2/auth/login`.

## Configuracao no servidor

No painel administrativo, abra `Configuracoes do Servidor` e preencha `Google OpenID Connect`:

- `Habilitado`: ativa o fluxo.
- `Client ID`: OAuth Client ID do Google.
- `Client Secret`: segredo do OAuth Client.
- `Redirect URI`: normalmente `https://SEU_HOST/api/v2/auth/google/callback`.
- `Retorno web/PWA`: URL da PWA que recebera `googleSession` por query string.
- `Codigo fixo de state`: segredo fixo com pelo menos 16 caracteres. Ele e embutido no `state` protegido para vincular a resposta ao servidor e a sessao iniciada.

No Google Cloud Console, cadastre a mesma `Redirect URI` no OAuth Client.

## Contratos

### Iniciar login

`POST /api/v2/auth/google/start`

```json
{
  "userAgent": "web",
  "clientInstanceId": "device-or-installation-id",
  "returnUrl": "https://app.exemplo.com/auth/google/callback"
}
```

Resposta:

```json
{
  "sessionCode": "0196...",
  "authorizationUrl": "https://accounts.google.com/o/oauth2/v2/auth?...",
  "expiresAt": "2026-05-03T20:10:00Z"
}
```

Abra `authorizationUrl` no navegador.

Tambem existe `GET /api/v2/auth/google/start?userAgent=web&clientInstanceId=...&returnUrl=...`, que ja redireciona o navegador ao Google.

### Consultar status

`GET /api/v2/auth/google/status?sessionCode=...&userAgent=windows&clientInstanceId=...`

Status possiveis:

- `Pending`: usuario ainda nao concluiu o Google.
- `Success`: tokens prontos para consumo.
- `Failed`: erro/cancelamento.
- `Consumed`: a sessao ja foi consumida.

### Consumir tokens

`POST /api/v2/auth/google/consume`

```json
{
  "sessionCode": "0196...",
  "userAgent": "web",
  "clientInstanceId": "device-or-installation-id"
}
```

Resposta: o mesmo `LoginResponse` do login por senha.

```json
{
  "token": "jwt",
  "refreshToken": "refresh",
  "tokenExpiresAt": "2026-05-03T23:00:00Z",
  "refreshTokenExpiresAt": "2026-06-02T19:00:00Z"
}
```

Para `userAgent = web`, o servidor tambem grava o refresh token no cookie HttpOnly `refresh_token`, mantendo o comportamento atual da API.

## PWA / Blazor WASM

1. Gere ou recupere um `clientInstanceId` persistente por instalacao, por exemplo em `localStorage`.
2. Chame `POST /api/v2/auth/google/start` com `userAgent = "web"` e `returnUrl` apontando para uma rota da PWA.
3. Redirecione o browser para `authorizationUrl`.
4. Quando a PWA voltar com `?googleSession=...&success=true`, chame `POST /api/v2/auth/google/consume`.
5. Armazene `token` como access token e use `Authorization: Bearer`.
6. Renove com `/api/v2/auth/refresh` como ja ocorre hoje.

Exemplo C#:

```csharp
var start = await http.PostAsJsonAsync("/api/v2/auth/google/start", new
{
    userAgent = "web",
    clientInstanceId,
    returnUrl = "https://app.exemplo.com/auth/google/callback"
});

var startBody = await start.Content.ReadFromJsonAsync<GoogleAuthStartResponse>();
navigation.NavigateTo(startBody!.AuthorizationUrl, forceLoad: true);
```

## Delphi Windows / Android

1. Gere e persista um `clientInstanceId` unico por instalacao.
2. Chame `POST /api/v2/auth/google/start` com `userAgent = "windows"` ou `"android"`.
3. Abra `authorizationUrl` no browser do sistema.
4. Quando a aplicacao voltar ao foco, consulte `/api/v2/auth/google/status`.
5. Se `Status = "Success"`, chame `/api/v2/auth/google/consume`.
6. Persista `token`, `refreshToken` e expiracoes no storage seguro disponivel.

O callback do servidor exibe uma pagina simples dizendo para fechar o browser. Se o cliente tiver deep link proprio, envie esse deep link em `returnUrl`; o servidor acrescentara `googleSession`, `success` e, em falhas, `error`.

Pseudo-fluxo Delphi:

```text
POST /auth/google/start
OpenURL(authorizationUrl)

OnAppActivated:
  GET /auth/google/status
  if status = Success:
    POST /auth/google/consume
    salvar JWT e refresh token
  if status = Failed:
    exibir mensagem
```

## Observacoes de seguranca

- O `state` contem o codigo fixo configurado, o `sessionCode`, o tipo de cliente e o `clientInstanceId`, tudo protegido pelo `SecretProtector`.
- A resposta de tokens fica armazenada temporariamente em `external_auth_sessions` protegida pelo mesmo mecanismo de segredos.
- A sessao expira em 10 minutos e so pode ser consumida uma vez.
- O servidor valida o `id_token` no endpoint `tokeninfo` do Google, confere `aud`, `iss`, `sub`, `email` e `email_verified`.
