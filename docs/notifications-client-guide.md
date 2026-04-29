# eTasks Notifications - Guia de uso para clientes

Este guia explica como clientes Delphi, Blazor WASM PWA, Android e Windows devem usar a central REST de notificacoes do eTasks.

## Estado atual

A implementacao atual entrega uma central de notificacoes via API REST:

- o painel administrativo envia mensagens para usuarios ou grupos;
- cada cliente autenticado lista suas notificacoes;
- cada cliente marca notificacoes como lidas;
- cada cliente registra seu dispositivo/token para uso futuro com push nativo.

Importante: nesta fase, a entrega garantida e via consulta REST. O registro de `pushToken`, `pushEndpoint`, `p256dh` e `auth` ja prepara o servidor para uma etapa posterior com Web Push, FCM ou outro provedor.

## Autenticacao

Todas as rotas de cliente exigem JWT Bearer.

Header:

```http
Authorization: Bearer {token}
Content-Type: application/json
```

O token vem do login:

```http
POST /api/v2/auth/login
```

## Plataformas

Valores aceitos em `platform`:

| Valor | Nome | Uso esperado |
|---:|---|---|
| 1 | `Pwa` | Blazor WASM PWA / navegador |
| 2 | `DelphiWindows` | App Delphi Windows |
| 3 | `DelphiAndroid` | App Delphi Android |
| 4 | `Android` | Android generico |
| 5 | `Windows` | Windows generico |
| 99 | `Other` | Outro cliente |

## Registrar dispositivo

Use esta rota apos login e sempre que o token do dispositivo mudar.

```http
POST /api/v2/notifications/devices
```

Body minimo:

```json
{
  "deviceId": "desktop-usuario-123",
  "platform": 2,
  "displayName": "Notebook do escritorio"
}
```

Body para PWA com Web Push:

```json
{
  "deviceId": "browser-installation-id",
  "platform": 1,
  "displayName": "Chrome PWA",
  "pushEndpoint": "https://fcm.googleapis.com/fcm/send/...",
  "p256dh": "base64-public-key",
  "auth": "base64-auth-secret"
}
```

Body para FCM:

```json
{
  "deviceId": "android-device-id",
  "platform": 3,
  "displayName": "Celular Android",
  "pushToken": "fcm-registration-token"
}
```

Resposta:

```json
{
  "id": "0f018f2d-2f49-7c58-9b1a-06f9a03c94ff",
  "platform": 2,
  "deviceId": "desktop-usuario-123",
  "displayName": "Notebook do escritorio",
  "isActive": true,
  "lastSeenAt": "2026-04-29T10:30:00"
}
```

## Listar notificacoes

```http
GET /api/v2/notifications
```

Somente nao lidas:

```http
GET /api/v2/notifications?unreadOnly=true
```

Resposta:

```json
[
  {
    "recipientId": "0f018f2d-30d1-7bdb-a19f-49df7a105c2a",
    "notificationId": "0f018f2d-30d0-7730-a8f5-1a4fb11f663d",
    "title": "Nova versao disponivel",
    "body": "Atualize o eTasks para receber as ultimas melhorias.",
    "actionUrl": "/version",
    "dataJson": "{ \"type\": \"version\" }",
    "createdAt": "2026-04-29T10:30:00",
    "readAt": null,
    "isRead": false
  }
]
```

## Contar nao lidas

```http
GET /api/v2/notifications/unread-count
```

Resposta:

```json
{
  "count": 3
}
```

## Marcar como lida

```http
PATCH /api/v2/notifications/{recipientId}/read
```

## Marcar todas como lidas

```http
PATCH /api/v2/notifications/read-all
```

## Estrategia recomendada por cliente

Para Delphi Windows:

- registrar o dispositivo apos login;
- fazer polling em `/api/v2/notifications/unread-count` a cada 30 a 60 segundos;
- quando houver notificacoes, chamar `/api/v2/notifications?unreadOnly=true`;
- exibir notificacao local no Windows;
- marcar como lida quando o usuario abrir ou dispensar.

Para Delphi Android:

- registrar o dispositivo apos login;
- se houver FCM no app, enviar `pushToken`;
- enquanto FCM nao estiver conectado no backend, usar polling REST;
- marcar leitura ao abrir a notificacao.

Para Blazor WASM PWA:

- registrar uma instalacao persistente no `localStorage`;
- opcionalmente solicitar permissao de notificacao do navegador;
- se ja houver Service Worker com Push API, enviar `pushEndpoint`, `p256dh` e `auth`;
- enquanto Web Push nao estiver conectado no backend, usar polling REST.

## Exemplo Delphi

Exemplo usando `TNetHTTPClient`.

```pascal
uses
  System.SysUtils, System.JSON, System.Net.HttpClient, System.Net.URLClient;

procedure RegisterDevice(const BaseUrl, JwtToken, DeviceId: string);
var
  Client: TNetHTTPClient;
  Body: TStringStream;
  Json: TJSONObject;
begin
  Client := TNetHTTPClient.Create(nil);
  try
    Client.CustomHeaders['Authorization'] := 'Bearer ' + JwtToken;
    Client.ContentType := 'application/json';

    Json := TJSONObject.Create;
    try
      Json.AddPair('deviceId', DeviceId);
      Json.AddPair('platform', TJSONNumber.Create(2)); // DelphiWindows
      Json.AddPair('displayName', 'Delphi Windows');

      Body := TStringStream.Create(Json.ToJSON, TEncoding.UTF8);
      try
        Client.Post(BaseUrl + '/api/v2/notifications/devices', Body);
      finally
        Body.Free;
      end;
    finally
      Json.Free;
    end;
  finally
    Client.Free;
  end;
end;
```

Buscar notificacoes nao lidas:

```pascal
function GetUnreadNotifications(const BaseUrl, JwtToken: string): string;
var
  Client: TNetHTTPClient;
  Response: IHTTPResponse;
begin
  Client := TNetHTTPClient.Create(nil);
  try
    Client.CustomHeaders['Authorization'] := 'Bearer ' + JwtToken;
    Response := Client.Get(BaseUrl + '/api/v2/notifications?unreadOnly=true');
    Result := Response.ContentAsString(TEncoding.UTF8);
  finally
    Client.Free;
  end;
end;
```

Marcar como lida:

```pascal
procedure MarkNotificationAsRead(const BaseUrl, JwtToken, RecipientId: string);
var
  Client: TNetHTTPClient;
begin
  Client := TNetHTTPClient.Create(nil);
  try
    Client.CustomHeaders['Authorization'] := 'Bearer ' + JwtToken;
    Client.Patch(BaseUrl + '/api/v2/notifications/' + RecipientId + '/read', nil);
  finally
    Client.Free;
  end;
end;
```

## Exemplo Blazor WASM PWA

DTOs:

```csharp
public sealed class RegisterPushDeviceRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public int Platform { get; set; } = 1;
    public string? DisplayName { get; set; }
    public string? PushToken { get; set; }
    public string? PushEndpoint { get; set; }
    public string? P256dh { get; set; }
    public string? Auth { get; set; }
}

public sealed class NotificationInboxItem
{
    public Guid RecipientId { get; set; }
    public Guid NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? DataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; }
}
```

Servico:

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

public sealed class ETasksNotificationsClient(HttpClient http)
{
    public void SetBearerToken(string token)
    {
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task RegisterPwaAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var request = new RegisterPushDeviceRequest
        {
            DeviceId = deviceId,
            Platform = 1,
            DisplayName = "Blazor WASM PWA"
        };

        await http.PostAsJsonAsync("/api/v2/notifications/devices", request, cancellationToken);
    }

    public async Task<List<NotificationInboxItem>> GetUnreadAsync(CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<List<NotificationInboxItem>>(
            "/api/v2/notifications?unreadOnly=true",
            cancellationToken) ?? [];
    }

    public async Task MarkAsReadAsync(Guid recipientId, CancellationToken cancellationToken = default)
    {
        await http.PatchAsync($"/api/v2/notifications/{recipientId}/read", null, cancellationToken);
    }
}
```

Gerar `deviceId` persistente:

```csharp
using Microsoft.JSInterop;

public static async Task<string> GetOrCreateDeviceIdAsync(IJSRuntime js)
{
    const string key = "etasks-device-id";
    var current = await js.InvokeAsync<string?>("localStorage.getItem", key);
    if (!string.IsNullOrWhiteSpace(current))
    {
        return current;
    }

    var created = Guid.NewGuid().ToString("N");
    await js.InvokeVoidAsync("localStorage.setItem", key, created);
    return created;
}
```

Polling simples:

```csharp
private PeriodicTimer? _timer;

private async Task StartNotificationPollingAsync(ETasksNotificationsClient client, CancellationToken cancellationToken)
{
    _timer = new PeriodicTimer(TimeSpan.FromSeconds(45));

    while (await _timer.WaitForNextTickAsync(cancellationToken))
    {
        var unread = await client.GetUnreadAsync(cancellationToken);
        foreach (var item in unread)
        {
            // Atualize badge, abra snackbar, ou use JS Notification API.
            Console.WriteLine($"{item.Title}: {item.Body}");
        }
    }
}
```

## Envio pelo painel

No painel administrativo:

```text
/admin/notifications
```

O envio grava a mensagem em `notification_messages` e cria os destinatarios em `notification_recipients`.

Grupos disponiveis:

- todos os usuarios ativos;
- usuarios comuns;
- administradores;
- usuarios selecionados.

## Proxima etapa para push nativo

Para transformar a central REST em push real:

- PWA: adicionar VAPID e envio Web Push para `pushEndpoint`, `p256dh` e `auth`;
- Android/Delphi Android: adicionar FCM e enviar para `pushToken`;
- Delphi Windows: manter polling REST, usar WebSocket/SSE, ou integrar com Windows Push Notification Services se o app for empacotado para Microsoft Store.

Mesmo com push nativo, mantenha a consulta REST como fonte de verdade. Push deve ser tratado como sinal para sincronizar, nao como unico armazenamento da mensagem.
