using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.Json;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace eTasks_server.Core.Services
{
    public class ServerSettingsDiagnosticsService(
        IHttpClientFactory httpClientFactory) : IServerSettingsDiagnosticsService
    {
        public async Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.SmtpEnabled)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Habilite o SMTP antes de testar." };
            }

            if (string.IsNullOrWhiteSpace(request.SmtpHost))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Informe o host SMTP." };
            }

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(request.SmtpHost.Trim(), request.SmtpPort, cancellationToken);

            if (request.SmtpEnableSsl)
            {
                using var sslStream = new SslStream(tcpClient.GetStream(), false);
                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = request.SmtpHost.Trim()
                }, cancellationToken);
            }

            return new ServerSettingsTestResultResponse
            {
                Success = true,
                Message = request.SmtpEnableSsl
                    ? "Conexao SMTP com handshake TLS concluida."
                    : "Conexao TCP com o servidor SMTP concluida."
            };
        }

        public async Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.OpenRouterEnabled)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Habilite a IA antes de testar." };
            }

            if (string.IsNullOrWhiteSpace(request.OpenRouterApiKey))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Informe a API key do OpenRouter." };
            }

            using var httpClient = httpClientFactory.CreateClient("OpenRouter");
            httpClient.BaseAddress = new Uri(NormalizeBaseUrl(request.OpenRouterBaseUrl));

            using var message = new HttpRequestMessage(HttpMethod.Get, "models");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.OpenRouterApiKey.Trim());

            if (!string.IsNullOrWhiteSpace(request.OpenRouterSiteUrl))
            {
                message.Headers.TryAddWithoutValidation("HTTP-Referer", request.OpenRouterSiteUrl.Trim());
            }

            if (!string.IsNullOrWhiteSpace(request.OpenRouterAppName))
            {
                message.Headers.TryAddWithoutValidation("X-Title", request.OpenRouterAppName.Trim());
            }

            using var response = await httpClient.SendAsync(message, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ServerSettingsTestResultResponse
                {
                    Success = false,
                    Message = $"OpenRouter retornou status {(int)response.StatusCode}."
                };
            }

            using var json = JsonDocument.Parse(content);
            var modelFound = json.RootElement.TryGetProperty("data", out var dataElement)
                && dataElement.ValueKind == JsonValueKind.Array
                && dataElement.EnumerateArray().Any(item =>
                    item.TryGetProperty("id", out var idElement)
                    && string.Equals(idElement.GetString(), request.OpenRouterModel.Trim(), StringComparison.OrdinalIgnoreCase));

            return new ServerSettingsTestResultResponse
            {
                Success = true,
                Message = modelFound
                    ? "Conexao com OpenRouter validada e modelo localizado."
                    : "Conexao com OpenRouter validada. O modelo informado nao apareceu na listagem retornada."
            };
        }

        public async Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.MongoAuditEnabled)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Habilite o MongoDB antes de testar." };
            }

            if (string.IsNullOrWhiteSpace(request.MongoAuditConnectionString))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Informe a connection string do MongoDB." };
            }

            var client = new MongoClient(request.MongoAuditConnectionString.Trim());
            var databaseName = string.IsNullOrWhiteSpace(request.MongoAuditDatabaseName)
                ? "admin"
                : request.MongoAuditDatabaseName.Trim();

            var database = client.GetDatabase(databaseName);
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);

            return new ServerSettingsTestResultResponse
            {
                Success = true,
                Message = $"Conexao com MongoDB validada no banco '{databaseName}'."
            };
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "https://openrouter.ai/api/v1/";
            }

            return baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
        }
    }
}
