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
    /// <summary>
    /// Classe responsável por fornecer serviços de assistência de IA usando o provedor OpenRouter.
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="promptComposer"></param>
    /// <param name="settingsProvider"></param>
    /// <param name="logger"></param>
    public class OpenRouterAiAssistantService(
        IHttpClientFactory httpClientFactory,
        IAiPromptComposer promptComposer,
        IServerSettingsProvider settingsProvider,
        ILogger<IAiAssistantService> logger) : IAiAssistantService
    {
        /// <summary>
        /// Fornece assistência de IA para um usuário específico com base na solicitação fornecida.
        /// </summary>
        /// <param name="userUid">O UID do usuário para o qual fornecer assistência.</param>
        /// <param name="request">A solicitação de assistência de IA.</param>
        /// <param name="cancellationToken">O token de cancelamento.</param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<AiAssistResponse> AssistAsync(Guid userUid, AiAssistRequest request, CancellationToken cancellationToken = default)
        {
            // Valida a solicitação de assistência de IA.
            ValidateRequest(request);

            // Obtém as configurações atuais do servidor.
            var settings = await settingsProvider.GetCurrentAsync(cancellationToken);

            // Verifica se o serviço OpenRouter está habilitado e se a chave de API está presente.
            if (!settings.OpenRouterEnabled || string.IsNullOrWhiteSpace(settings.OpenRouterApiKey))
            {
                throw new ApiException(System.Net.HttpStatusCode.ServiceUnavailable, "O servico de IA não está habilitado no servidor.");
            }

            // Cria um cliente HTTP para se comunicar com o serviço OpenRouter.
            var httpClient = httpClientFactory.CreateClient("OpenRouter");
            httpClient.BaseAddress = new Uri(NormalizeBaseUrl(settings.OpenRouterBaseUrl));

            // Cria uma mensagem HTTP POST para enviar a solicitação de assistência de IA.
            using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.OpenRouterApiKey);

            // Adiciona cabeçalhos opcionais, se fornecidos nas configurações.
            if (!string.IsNullOrWhiteSpace(settings.OpenRouterSiteUrl))
            {
                message.Headers.TryAddWithoutValidation("HTTP-Referer", settings.OpenRouterSiteUrl);
            }

            // Adiciona o nome do aplicativo, se fornecido nas configurações.
            if (!string.IsNullOrWhiteSpace(settings.OpenRouterAppName))
            {
                message.Headers.TryAddWithoutValidation("X-Title", settings.OpenRouterAppName);
            }

            // Adiciona a versão do aplicativo, se fornecida nas configurações.
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

            // Serializa o payload da solicitação para JSON e adiciona ao corpo da mensagem HTTP.
            message.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Envia a solicitação HTTP para o serviço OpenRouter e aguarda a resposta.
            var response = await httpClient.SendAsync(message, cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Verifica se a resposta foi bem-sucedida; caso contrário, registra um aviso e lança uma exceção.
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Falha ao chamar OpenRouter para usuário {UserUid}. Status {StatusCode}. Conteúdo: {Content}", userUid, (int)response.StatusCode, rawContent);
                throw new ApiException(response.StatusCode, "Falha ao processar a solicitação de IA no provedor externo.");
            }

            // Desserializa a resposta JSON do serviço OpenRouter.
            var parsed = JsonSerializer.Deserialize<OpenRouterChatResponse>(rawContent, JsonSerializerOptions.Web)
                ?? throw new ApiException(System.Net.HttpStatusCode.BadGateway, "Resposta inválida do provedor de IA.");

            // Verifica se o conteúdo retornado pelo provedor de IA é útil; caso contrário, lança uma exceção.
            var content = parsed.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ApiException(System.Net.HttpStatusCode.BadGateway, "O provedor de IA não retornou conteúdo útil.");
            }

            // Retorna a resposta de assistência de IA com informações sobre o provedor, modelo, conteúdo e uso de tokens.
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

        /// <summary>
        /// Normaliza a URL base fornecida, garantindo que ela termine com uma barra ('/'). Se a URL base for nula ou vazia, retorna a URL padrão do OpenRouter.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        private static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "https://openrouter.ai/api/v1/";
            }

            return baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
        }

        /// <summary>
        /// Valida a solicitação de assistência de IA, verificando se os campos obrigatórios estão presentes e se os valores estão dentro dos limites permitidos. Lança uma exceção de validação se algum campo estiver inválido.
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateRequest(AiAssistRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserPrompt))
            {
                throw new ValidationException("UserPrompt", "A solicitação para a IA é obrigatória.");
            }

            if (request.UserPrompt.Trim().Length > 4000)
            {
                throw new ValidationException("UserPrompt", "A solicitação para a IA deve ter no máximo 4000 caracteres.");
            }

            if (!Enum.IsDefined(request.Resource))
            {
                throw new ValidationException("Resource", "Recurso de IA inválido.");
            }

            if (!Enum.IsDefined(request.Intent))
            {
                throw new ValidationException("Intent", "Intenção de IA inválida.");
            }
        }

        /// <summary>
        /// Normaliza o papel (role) da mensagem, convertendo-o para minúsculas e mapeando valores conhecidos para os papéis esperados pelo provedor OpenRouter. Se o papel não for reconhecido, será tratado como "user".
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        private static string NormalizeRole(string role)
        {
            return role.Trim().ToLowerInvariant() switch
            {
                "assistant" => "assistant",
                "system" => "system",
                _ => "user"
            };
        }

        /// <summary>
        /// Modelo de solicitação para o serviço de chat do OpenRouter, contendo informações sobre o modelo, mensagens, temperatura e limite de tokens.
        /// </summary>
        private sealed class OpenRouterChatRequest
        {
            /// <summary>
            /// Modelo de IA a ser utilizado pelo serviço de chat do OpenRouter para gerar a resposta. O modelo deve ser especificado como uma string não vazia, representando o nome do modelo desejado.
            /// </summary>
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            /// <summary>
            /// Mensagens enviadas para o serviço de chat do OpenRouter, representando o diálogo entre o usuário, o sistema e o assistente de IA.
            /// </summary>
            [JsonPropertyName("messages")]
            public List<OpenRouterChatMessage> Messages { get; set; } = [];

            /// <summary>
            /// Temperatura do modelo de IA, que controla a aleatoriedade das respostas geradas. Valores mais baixos resultam em respostas mais determinísticas, enquanto valores mais altos aumentam a diversidade das respostas.
            /// </summary>
            [JsonPropertyName("temperature")]
            public decimal Temperature { get; set; }

            /// <summary>
            /// Máximo de tokens permitidos na resposta gerada pelo modelo de IA. Esse valor define o limite de comprimento da resposta e ajuda a controlar o custo e o tempo de processamento da solicitação.
            /// </summary>
            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }
        }

        /// <summary>
        /// Modelo de mensagem para o serviço de chat do OpenRouter, contendo informações sobre o papel (role) e o conteúdo da mensagem.
        /// </summary>
        private sealed class OpenRouterChatMessage
        {
            /// <summary>
            /// Papel (role) da mensagem, que pode ser "system", "user" ou "assistant". O papel define o contexto da mensagem no diálogo com o modelo de IA.
            /// </summary>
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            /// <summary>
            /// Conteúdo da mensagem, que representa o texto enviado pelo usuário ou pelo sistema para o modelo de IA. O conteúdo deve ser uma string não vazia e pode conter instruções, perguntas ou informações relevantes para a assistência de IA.
            /// </summary>
            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        /// <summary>
        /// Modelo de resposta do serviço de chat do OpenRouter, contendo informações sobre o modelo utilizado, as escolhas retornadas e o uso de tokens.
        /// </summary>
        private sealed class OpenRouterChatResponse
        {
            /// <summary>
            /// Nome do modelo utilizado pelo serviço de chat do OpenRouter para gerar a resposta.
            /// </summary>
            [JsonPropertyName("model")]
            public string? Model { get; set; }

            /// <summary>
            /// Lista de escolhas retornadas pelo serviço de chat do OpenRouter, cada uma contendo uma mensagem associada.
            /// </summary>
            [JsonPropertyName("choices")]
            public List<OpenRouterChoice>? Choices { get; set; }

            /// <summary>
            /// Informações sobre o uso de tokens retornadas pelo serviço de chat do OpenRouter, incluindo o número de tokens utilizados no prompt, na conclusão e o total de tokens.
            /// </summary>
            [JsonPropertyName("usage")]
            public OpenRouterUsage? Usage { get; set; }
        }

        /// <summary>
        /// Modelo de escolha retornada pelo serviço de chat do OpenRouter, contendo a mensagem associada à escolha.
        /// </summary>
        private sealed class OpenRouterChoice
        {
            /// <summary>
            /// Mensagem associada à escolha retornada pelo serviço de chat do OpenRouter.
            /// </summary>
            [JsonPropertyName("message")]
            public OpenRouterChatMessage? Message { get; set; }
        }

        /// <summary>
        /// Modelo de uso de tokens retornado pelo serviço de chat do OpenRouter, contendo informações sobre o número de tokens utilizados no prompt, na conclusão e o total de tokens.
        /// </summary>
        private sealed class OpenRouterUsage
        {
            /// <summary>
            /// Número de tokens utilizados no prompt enviado para o serviço de IA.
            /// </summary>
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            /// <summary>
            /// Número de tokens utilizados na resposta gerada pelo serviço de IA.
            /// </summary>
            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            /// <summary>
            /// Número total de tokens utilizados, incluindo o prompt e a conclusão.
            /// </summary>
            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}
