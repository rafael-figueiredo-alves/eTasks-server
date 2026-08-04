using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Classe responsável por fornecer serviços de diagnóstico para as configurações do servidor, incluindo testes de conexão SMTP, OpenRouter e MongoDB.
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="secretProtector"></param>
    public class ServerSettingsDiagnosticsService(
        IHttpClientFactory httpClientFactory, ISecretProtector secretProtector) : IServerSettingsDiagnosticsService
    {
        /// <summary>
        /// Testa a conexão com o servidor SMTP usando as configurações fornecidas no request. Valida se o SMTP está habilitado, se o host e a porta foram informados, realiza handshake TLS se necessário e tenta autenticar com as credenciais fornecidas.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ServerSettingsTestResultResponse> TestEmailAsync(
            UpdateServerSettingsRequest request,
            CancellationToken cancellationToken = default)
        {
            // Valida se o SMTP está habilitado
            if (!request.SmtpEnabled)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Habilite o SMTP antes de testar." };
            }

            // Valida se o host SMTP foi informado
            if (string.IsNullOrWhiteSpace(request.SmtpHost))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Informe o host SMTP." };
            }

            if (request.SmtpPort <= 0 || request.SmtpPort > 65535)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Porta SMTP inválida." };
            }

            var host = request.SmtpHost.Trim();

            // Timeout de segurança para não travar caso a porta esteja filtrada por firewall
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(host, request.SmtpPort, timeoutCts.Token);

                Stream stream = tcpClient.GetStream();
                SslStream? sslStream = null;

                // Porta 465 = TLS implícito (conecta já criptografado)
                // Porta 587/25 = geralmente texto plano + comando STARTTLS
                var useImplicitTls = request.SmtpEnableSsl && request.SmtpPort == 465;

                if (useImplicitTls)
                {
                    sslStream = new SslStream(stream, false);
                    await sslStream.AuthenticateAsClientAsync(
                        new SslClientAuthenticationOptions { TargetHost = host },
                        timeoutCts.Token);
                    stream = sslStream;
                }

                using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, leaveOpen: true);
                using var writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen: true) { AutoFlush = true, NewLine = "\r\n" };

                // Lê o banner de saudação (deve começar com "220")
                var banner = await reader.ReadLineAsync(timeoutCts.Token);
                if (banner is null || !banner.StartsWith("220"))
                {
                    return new ServerSettingsTestResultResponse
                    {
                        Success = false,
                        Message = $"O servidor não respondeu como um SMTP válido. Resposta: {banner ?? "(vazia)"}"
                    };
                }

                // Envia EHLO para negociar capacidades
                await writer.WriteLineAsync($"EHLO {Environment.MachineName}");
                var ehloResponse = await ReadFullResponseAsync(reader, timeoutCts.Token);
                if (!ehloResponse.StartsWith("250"))
                {
                    return new ServerSettingsTestResultResponse { Success = false, Message = $"Falha no EHLO: {ehloResponse}" };
                }

                // Se SSL habilitado e não for TLS implícito, tenta STARTTLS
                if (request.SmtpEnableSsl && !useImplicitTls)
                {
                    await writer.WriteLineAsync("STARTTLS");
                    var startTlsResponse = await reader.ReadLineAsync(timeoutCts.Token);
                    if (startTlsResponse is null || !startTlsResponse.StartsWith("220"))
                    {
                        return new ServerSettingsTestResultResponse { Success = false, Message = $"Falha no STARTTLS: {startTlsResponse}" };
                    }

                    sslStream = new SslStream(stream, false);
                    await sslStream.AuthenticateAsClientAsync(
                        new SslClientAuthenticationOptions { TargetHost = host },
                        timeoutCts.Token);

                    // Após STARTTLS o protocolo exige um novo EHLO dentro do canal criptografado
                    using var secureReader = new StreamReader(sslStream, Encoding.ASCII, false, 1024, leaveOpen: true);
                    using var secureWriter = new StreamWriter(sslStream, Encoding.ASCII, 1024, leaveOpen: true) { AutoFlush = true, NewLine = "\r\n" };

                    await secureWriter.WriteLineAsync($"EHLO {Environment.MachineName}");
                    var secureEhlo = await ReadFullResponseAsync(secureReader, timeoutCts.Token);
                    if (!secureEhlo.StartsWith("250"))
                    {
                        return new ServerSettingsTestResultResponse { Success = false, Message = $"Falha no EHLO pós-STARTTLS: {secureEhlo}" };
                    }

                    // Testa autenticação se credenciais foram informadas
                    if (!string.IsNullOrWhiteSpace(request.SmtpUsername) && !string.IsNullOrWhiteSpace(request.SmtpPassword))
                    {
                        var authResult = await TryAuthenticateAsync(secureReader, secureWriter, request.SmtpUsername, secretProtector.Unprotect(request.SmtpPassword), timeoutCts.Token);
                        if (!authResult.Success)
                        {
                            return authResult;
                        }
                    }

                    await secureWriter.WriteLineAsync("QUIT");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.SmtpUsername) && !string.IsNullOrWhiteSpace(request.SmtpPassword))
                    {
                        var authResult = await TryAuthenticateAsync(reader, writer, request.SmtpUsername, secretProtector.Unprotect(request.SmtpPassword), timeoutCts.Token);
                        if (!authResult.Success)
                        {
                            return authResult;
                        }
                    }

                    await writer.WriteLineAsync("QUIT");
                }

                return new ServerSettingsTestResultResponse
                {
                    Success = true,
                    Message = request.SmtpEnableSsl
                        ? "Conexão SMTP com handshake TLS e EHLO concluídos com sucesso."
                        : "Conexão SMTP concluída com sucesso (sem criptografia)."
                };
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Tempo limite excedido ao conectar no servidor SMTP." };
            }
            catch (SocketException ex)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = $"Não foi possível conectar ao host/porta informados: {ex.Message}" };
            }
            catch (AuthenticationException ex)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = $"Falha no handshake TLS: {ex.Message}" };
            }
            catch (IOException ex)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = $"Erro de comunicação com o servidor: {ex.Message}" };
            }
        }

        /// <summary>
        /// Testa a conexão com o OpenRouter usando as configurações fornecidas no request. Valida se o OpenRouter está habilitado, se a API key foi informada e se o modelo especificado está disponível.
        /// </summary>
        /// <param name="request">Dados de configuração do OpenRouter</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida se o OpenRouter está habilitado
            if (!request.OpenRouterEnabled)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Habilite a IA antes de testar." };
            }

            // Valida se a API key do OpenRouter foi informada
            if (string.IsNullOrWhiteSpace(request.OpenRouterApiKey))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Informe a API key do OpenRouter." };
            }

            // Valida se o modelo do OpenRouter foi informado
            using var httpClient = httpClientFactory.CreateClient("OpenRouter");
            httpClient.BaseAddress = new Uri(NormalizeBaseUrl(request.OpenRouterBaseUrl));

            using var message = new HttpRequestMessage(HttpMethod.Get, "models");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.OpenRouterApiKey.Trim());

            // Adiciona cabeçalhos opcionais se fornecidos
            if (!string.IsNullOrWhiteSpace(request.OpenRouterSiteUrl))
            {
                message.Headers.TryAddWithoutValidation("HTTP-Referer", request.OpenRouterSiteUrl.Trim());
            }

            // Adiciona cabeçalhos opcionais se fornecidos
            if (!string.IsNullOrWhiteSpace(request.OpenRouterAppName))
            {
                message.Headers.TryAddWithoutValidation("X-Title", request.OpenRouterAppName.Trim());
            }

            // Adiciona cabeçalhos opcionais se fornecidos e envia a temperatura e o número máximo de tokens como cabeçalhos personalizados
            using var response = await httpClient.SendAsync(message, cancellationToken);

            // Lê o conteúdo da resposta como string
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Valida se a resposta do OpenRouter foi bem-sucedida
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
                    : "Conexao com OpenRouter validada. O modelo informado não apareceu na listagem retornada."
            };
        }

        /// <summary>
        /// Testa a conexão com o MongoDB usando as configurações fornecidas no request. Valida se o MongoDB está habilitado, se a connection string foi informada e se é possível realizar um ping no banco de dados especificado.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida se o MongoDB está habilitado
            if (!request.MongoAuditEnabled)
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Habilite o MongoDB antes de testar." };
            }

            // Valida se a connection string do MongoDB foi informada
            if (string.IsNullOrWhiteSpace(request.MongoAuditConnectionString))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = "Informe a connection string do MongoDB." };
            }

            // Valida se o nome do banco de dados do MongoDB foi informado
            var client = new MongoClient(request.MongoAuditConnectionString.Trim());

            // Se o nome do banco de dados não for informado, utiliza o banco "admin" como padrão
            var databaseName = string.IsNullOrWhiteSpace(request.MongoAuditDatabaseName)
                ? "admin"
                : request.MongoAuditDatabaseName.Trim();

            var database = client.GetDatabase(databaseName);

            // Realiza um ping no banco de dados para validar a conexão
            await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);

            // Se o ping for bem-sucedido, retorna sucesso
            return new ServerSettingsTestResultResponse
            {
                Success = true,
                Message = $"Conexão com MongoDB validada no banco '{databaseName}'."
            };
        }

        /// <summary>
        /// Lê a resposta completa do servidor SMTP, lidando com respostas multilinha que usam "250-" nas linhas intermediárias e "250 " na última linha.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<string> ReadFullResponseAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            // Respostas multilinha do SMTP usam "250-" nas linhas intermediárias e "250 " na última
            string? line;
            var lastLine = string.Empty;
            do
            {
                line = await reader.ReadLineAsync(cancellationToken);
                if (line is null) break;
                lastLine = line;
            } while (line.Length > 3 && line[3] == '-');

            return lastLine;
        }

        /// <summary>
        /// Tenta autenticar no servidor SMTP usando o método AUTH LOGIN com as credenciais fornecidas. Retorna sucesso se a autenticação for bem-sucedida, ou uma mensagem de erro caso contrário.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<ServerSettingsTestResultResponse> TryAuthenticateAsync(
            StreamReader reader, StreamWriter writer, string user, string password, CancellationToken cancellationToken)
        {
            await writer.WriteLineAsync("AUTH LOGIN");
            var authPrompt = await reader.ReadLineAsync(cancellationToken);
            if (authPrompt is null || !authPrompt.StartsWith("334"))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = $"Servidor não aceitou AUTH LOGIN: {authPrompt}" };
            }

            await writer.WriteLineAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(user)));
            var userPrompt = await reader.ReadLineAsync(cancellationToken);
            if (userPrompt is null || !userPrompt.StartsWith("334"))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = $"Usuário rejeitado: {userPrompt}" };
            }

            await writer.WriteLineAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
            var authResponse = await reader.ReadLineAsync(cancellationToken);
            if (authResponse is null || !authResponse.StartsWith("235"))
            {
                return new ServerSettingsTestResultResponse { Success = false, Message = $"Falha na autenticação: {authResponse}" };
            }

            return new ServerSettingsTestResultResponse { Success = true, Message = "Autenticado com sucesso." };
        }

        /// <summary>
        /// Método auxiliar para normalizar a URL base do OpenRouter, garantindo que ela termine com uma barra (/) e fornecendo um valor padrão caso a URL seja nula ou vazia.
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
    }
}
