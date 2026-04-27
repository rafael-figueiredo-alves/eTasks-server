using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.DTOs.AI.Responses;
using eTasks_server.Models.Exceptions;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.Services
{
    public class OpenRouterAiAssistantService(
        IHttpClientFactory httpClientFactory,
        IAiPromptComposer promptComposer,
        IServerSettingsProvider settingsProvider,
        ILogger<IAiAssistantService> logger) : IAiAssistantService
    {
        public async Task<AiAssistResponse> AssistAsync(Guid userUid, AiAssistRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);
            var settings = await settingsProvider.GetCurrentAsync(cancellationToken);

            if (!settings.OpenRouterEnabled || string.IsNullOrWhiteSpace(settings.OpenRouterApiKey))
            {
                throw new ApiException(System.Net.HttpStatusCode.ServiceUnavailable, "O servico de IA nao esta habilitado no servidor.");
            }

            var httpClient = httpClientFactory.CreateClient("OpenRouter");
            httpClient.BaseAddress = new Uri(NormalizeBaseUrl(settings.OpenRouterBaseUrl));

            using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.OpenRouterApiKey);

            if (!string.IsNullOrWhiteSpace(settings.OpenRouterSiteUrl))
            {
                message.Headers.TryAddWithoutValidation("HTTP-Referer", settings.OpenRouterSiteUrl);
            }

            if (!string.IsNullOrWhiteSpace(settings.OpenRouterAppName))
            {
                message.Headers.TryAddWithoutValidation("X-Title", settings.OpenRouterAppName);
            }

            var payload = new OpenRouterChatRequest
            {
                Model = settings.OpenRouterModel,
                Temperature = settings.OpenRouterTemperature,
                MaxTokens = settings.OpenRouterMaxTokens,
                Messages =
                [
                    new OpenRouterChatMessage
                    {
                        Role = "system",
                        Content = promptComposer.BuildSystemPrompt(request)
                    },
                    .. request.Conversation
                        .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                        .Select(x => new OpenRouterChatMessage
                        {
                            Role = NormalizeRole(x.Role),
                            Content = x.Content.Trim()
                        }),
                    new OpenRouterChatMessage
                    {
                        Role = "user",
                        Content = promptComposer.BuildUserPrompt(request)
                    }
                ]
            };

            message.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(message, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Falha ao chamar OpenRouter para usuario {UserUid}. Status {StatusCode}. Conteudo: {Content}", userUid, (int)response.StatusCode, rawContent);
                throw new ApiException(response.StatusCode, "Falha ao processar a solicitacao de IA no provedor externo.");
            }

            var parsed = JsonSerializer.Deserialize<OpenRouterChatResponse>(rawContent, JsonSerializerOptions.Web)
                ?? throw new ApiException(System.Net.HttpStatusCode.BadGateway, "Resposta invalida do provedor de IA.");

            var content = parsed.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ApiException(System.Net.HttpStatusCode.BadGateway, "O provedor de IA nao retornou conteudo util.");
            }

            return new AiAssistResponse
            {
                Provider = "OpenRouter",
                Model = parsed.Model ?? settings.OpenRouterModel,
                Content = content,
                Usage = new AiUsageResponse
                {
                    PromptTokens = parsed.Usage?.PromptTokens ?? 0,
                    CompletionTokens = parsed.Usage?.CompletionTokens ?? 0,
                    TotalTokens = parsed.Usage?.TotalTokens ?? 0
                }
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

        private static void ValidateRequest(AiAssistRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserPrompt))
            {
                throw new ValidationException("UserPrompt", "A solicitacao para a IA e obrigatoria.");
            }

            if (request.UserPrompt.Trim().Length > 4000)
            {
                throw new ValidationException("UserPrompt", "A solicitacao para a IA deve ter no maximo 4000 caracteres.");
            }

            if (!Enum.IsDefined(request.Resource))
            {
                throw new ValidationException("Resource", "Recurso de IA invalido.");
            }

            if (!Enum.IsDefined(request.Intent))
            {
                throw new ValidationException("Intent", "Intencao de IA invalida.");
            }
        }

        private static string NormalizeRole(string role)
        {
            return role.Trim().ToLowerInvariant() switch
            {
                "assistant" => "assistant",
                "system" => "system",
                _ => "user"
            };
        }

        private sealed class OpenRouterChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<OpenRouterChatMessage> Messages { get; set; } = [];

            [JsonPropertyName("temperature")]
            public decimal Temperature { get; set; }

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }
        }

        private sealed class OpenRouterChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private sealed class OpenRouterChatResponse
        {
            [JsonPropertyName("model")]
            public string? Model { get; set; }

            [JsonPropertyName("choices")]
            public List<OpenRouterChoice>? Choices { get; set; }

            [JsonPropertyName("usage")]
            public OpenRouterUsage? Usage { get; set; }
        }

        private sealed class OpenRouterChoice
        {
            [JsonPropertyName("message")]
            public OpenRouterChatMessage? Message { get; set; }
        }

        private sealed class OpenRouterUsage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}
