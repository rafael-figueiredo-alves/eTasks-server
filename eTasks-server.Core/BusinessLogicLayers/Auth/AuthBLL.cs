using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Helpers;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.DTOs.Auth.Responses;
using eTasks_server.Models.DTOs.GoogleAuth;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eTasks_server.Core.BusinessLogicLayers.Auth
{
    /// <summary>
    /// Classe de negócio responsável por gerenciar a autenticação e autorização da aplicação
    /// </summary>
    public class AuthBLL : BaseBLL<IAuthBLL>, IAuthBLL
    {
        #region Variáveis de serviços injetados
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISecretProtector _secretProtector;
        private readonly IServerSettingsProvider _serverSettingsProvider;
        private readonly IAccountDeletionRetentionService _accountDeletionRetentionService;
        private readonly IHttpClientFactory _httpClientFactory;
        #endregion

        private const string GoogleProvider = "google";

        #region Classes particulares referentes ao login com Google
        /// <summary>
        /// Classe que comporta dados do payload do state enviado ao Google
        /// </summary>
        private sealed class GoogleStatePayload
        {
            /// <summary>
            /// Código fixo de state
            /// </summary>
            public string FixedStateCode { get; set; } = string.Empty;

            /// <summary>
            /// Código da seção
            /// </summary>
            public Guid SessionCode { get; set; }

            /// <summary>
            /// Agente do cliente (identificador da plataforma)
            /// </summary>
            public string UserAgent { get; set; } = string.Empty;

            /// <summary>
            /// Id do cliente
            /// </summary>
            public string ClientInstanceId { get; set; } = string.Empty;

            /// <summary>
            /// URL de retorno
            /// </summary>
            public string? ReturnUrl { get; set; }

            /// <summary>
            /// Data e hora da criação
            /// </summary>
            public DateTime CreatedAtUtc { get; set; }
        }

        /// <summary>
        /// Resposta do token do Google
        /// </summary>
        private sealed class GoogleTokenResponse
        {
            /// <summary>
            /// Id do token
            /// </summary>
            [JsonPropertyName("id_token")]
            public string IdToken { get; set; } = string.Empty;
        }

        /// <summary>
        /// Informações do token do Google
        /// </summary>
        private sealed class GoogleTokenInfo
        {
            /// <summary>
            /// Solicitador do token
            /// </summary>
            [JsonPropertyName("iss")]
            public string Iss { get; set; } = string.Empty;

            /// <summary>
            /// Audiência do token
            /// </summary>
            [JsonPropertyName("aud")]
            public string Aud { get; set; } = string.Empty;

            /// <summary>
            /// Subscritos
            /// </summary>
            [JsonPropertyName("sub")]
            public string Sub { get; set; } = string.Empty;

            /// <summary>
            /// E-mail do Google
            /// </summary>
            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// E-mail verificado
            /// </summary>
            [JsonPropertyName("email_verified")]
            public string EmailVerified { get; set; } = "false";

            /// <summary>
            /// E-mail verificado
            /// </summary>
            [JsonIgnore]
            public bool IsEmailVerified => string.Equals(EmailVerified, "true", StringComparison.OrdinalIgnoreCase);

            /// <summary>
            /// Nome do usuário
            /// </summary>
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            /// <summary>
            /// Imagem / foto do usuário
            /// </summary>
            [JsonPropertyName("picture")]
            public string? Picture { get; set; }
        }
        #endregion

        /// <summary>
        /// Método construtor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        /// <param name="emailService"></param>
        /// <param name="secretProtector"></param>
        /// <param name="serverSettingsProvider"></param>
        /// <param name="accountDeletionRetentionService"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="logger"></param>
        public AuthBLL(
            AppDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ISecretProtector secretProtector,
            IServerSettingsProvider serverSettingsProvider,
            IAccountDeletionRetentionService accountDeletionRetentionService,
            IHttpClientFactory httpClientFactory,
            ILogger<IAuthBLL> logger) : base(context, logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _secretProtector = secretProtector;
            _serverSettingsProvider = serverSettingsProvider;
            _accountDeletionRetentionService = accountDeletionRetentionService;
            _httpClientFactory = httpClientFactory;
        }

        #region Métodos principais da Classe

        #region Métodos de Autenticação e Autorização com e-mail e senha (tradicional)
        /// <summary>
        /// Método responsável por registrar uma nova conta de usuário ao sistema
        /// </summary>
        /// <param name="request">Dados do usuário paar registr</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            // Informa etapa de registro no log
            _logger.LogInformation("Iniciando processo de registro para o e-mail: {Email}", request.Email);

            // Validando se e-mail ou senha estão vazios ou em branco
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Geral", "E-mail e senha são obrigatórios.");

            // Valida se o UserAgent veio preenchido
            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException("UserAgent", "O UserAgent é obrigatório.");

            // Valida se já existe conta com o e-mail informado
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Tentativa de registro falhou. O e-mail {Email} já está em uso.", request.Email);
                throw new ValidationException("Email", "O e-mail informado já está cadastrado.");
            }

            // Inicia gravação da imagem de perfil se enviada
            string? photoPath = null;
            if (!string.IsNullOrWhiteSpace(request.PhotoBase64))
            {
                try
                {
                    // Salva foto de perfil
                    photoPath = await UserPhotoStorage.SaveAsync(request.PhotoBase64, null);
                }
                catch
                {
                }
            }

            // Cria entidade de usuário
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.Password)),
                PhotoPath = photoPath,
                IsAdmin = false
            };

            // Adiciona a conta ao pipeline de criação do EF
            await _context.Users.AddAsync(user);

            // Adiciona os dados padrões das configurações
            await _context.UserSettings.AddAsync(new UserSettings { UserUid = user.Uid });

            // Salva tudo no banco
            await _context.SaveChangesAsync();

            // Informa no log
            _logger.LogInformation("Novo usuário criado no banco de dados. Uid: {Uid}, E-mail: {Email}", user.Uid, user.Email);

            // Envia e-mail sem aguardar retorno (sem await)
            #pragma warning disable CS4014
            _emailService.SendAccountConfirmationEmailAsync(user.Email, GetConfirmationLink(user.Uid.ToString());
            #pragma warning restore CS4014

            // Gera resposta com dados da conta e token JWT e Refresh Token para autenticação e autorização
            return await GenerateAuthResponseAsync(user, request.UserAgent);
        }

        /// <summary>
        /// Efetua login numa conta de usuário
        /// </summary>
        /// <param name="request">Dados para efetuar login</param>
        /// <param name="ipAddress">Endereço de IP</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="ApiException"></exception>
        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
        {
            // Informa tentativa de login no log
            _logger.LogInformation("Iniciando tentativa de login para o usuário: {Email}", request.Email);

            // Valida se foi passado usuário e senha
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Geral", "Forneça o e-mail e a senha para continuar.");

            // Valida se foi informado UserAgent
            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException("UserAgent", "O UserAgent é obrigatório.");

            // Tenta obter usuário informado
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Valida e retorna se usuário não existir no banco de dados
            if (user == null)
            {
                _logger.LogWarning("Falha de autenticação. E-mail não encontrado: {Email}", request.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = null, Status = "Failed", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Não encontramos uma conta com esse e-mail.");
            }

            // Valida se o usuário encontrado foi removido (soft delete)
            if (user.IsDeleted)
            {
                _logger.LogWarning("Usuário {Uid} ({Email}) tentou logar, mas a conta foi removida.", user.Uid, user.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Blocked", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi removida e não pode mais ser utilizada.");
            }

            // Valida a senha informada
            if (!BCrypt.Net.BCrypt.Verify(request.Password, _secretProtector.Unprotect(user.PasswordHash)))
            {
                _logger.LogWarning("Falha de autenticação. Senha incorreta para o usuário: {Uid}", user.Uid);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Failed", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Senha incorreta. Verifique e tente novamente.");
            }

            // Valida se usuário se encontra bloqueado para acessar o sistema
            if (user.IsBlocked)
            {
                _logger.LogWarning("Usuário {Uid} ({Email}) tentou logar, mas encontra-se bloqueado.", user.Uid, user.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Blocked", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            // Notifica login bem sucedido
            _logger.LogInformation("Usuário {Uid} autenticado com sucesso.", user.Uid);

            await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Success", IpAddress = ipAddress, UserAgent = request.UserAgent });
            user.LastAccessAt = SaoPauloDateTime.Now();

            // Retorna dados de autenticação e autorização (Token e refresh token)
            return await GenerateAuthResponseAsync(user, request.UserAgent);
        }
        #endregion

        #region Métodos de Autenticação e Autorização com Login do Google (OAUTH2)
        /// <summary>
        /// Inicia o login com a Conta do Google
        /// </summary>
        /// <param name="request">Parâmetros necessários para efetuar login</param>
        /// <param name="requestBaseUri">A url base</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<GoogleAuthStartResponse> StartGoogleLoginAsync(GoogleAuthStartRequest request, Uri requestBaseUri, CancellationToken cancellationToken = default)
        {
            // Valida se o User Agent foi informado para detectar qual plataforma está acessando o servidor
            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException(nameof(request.UserAgent), "O UserAgent é obrigatório.");

            // Valida se o user agent pertence a algum dos clientes do servidor, e não é requisição feita pelo Postman
            if (!Constants.ApiClientUserAgents.Contains(request.UserAgent, StringComparer.OrdinalIgnoreCase))
                throw new ValidationException(nameof(request.UserAgent), "O login com Google é permitido apenas para clientes da API.");

            // Valida se o identificador de cliente não está vazio
            if (string.IsNullOrWhiteSpace(request.ClientInstanceId))
                throw new ValidationException(nameof(request.ClientInstanceId), "O identificador do cliente é obrigatório.");

            // Pega as configurações do servidor 
            var settings = await _serverSettingsProvider.GetCurrentAsync(cancellationToken);

            // Valida configurações do Google nas configurações do servidor
            ValidateGoogleSettings(settings);

            // Gera a seção
            var session = new ExternalAuthSession
            {
                Provider = GoogleProvider,
                ClientUserAgent = request.UserAgent.Trim().ToLowerInvariant(),
                ClientInstanceId = request.ClientInstanceId.Trim(),
                FixedStateCode = settings.GoogleOpenIdStateCode.Trim(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            // Adiciona informações da seção com provedor Google de autenticação
            await _context.ExternalAuthSessions.AddAsync(session, cancellationToken);

            // Salva informações no banco de dados
            await _context.SaveChangesAsync(cancellationToken);

            // Obtém a URL de redirecionamento de acordo com cliente conectado
            var redirectUri = ResolveGoogleRedirectUri(settings.GoogleOpenIdRedirectUri, requestBaseUri);

            // Gera o parâmetro State
            var statePayload = new GoogleStatePayload
            {
                FixedStateCode = session.FixedStateCode,
                SessionCode = session.SessionCode,
                UserAgent = session.ClientUserAgent,
                ClientInstanceId = session.ClientInstanceId,
                ReturnUrl = request.ReturnUrl,
                CreatedAtUtc = DateTime.UtcNow
            };

            // Gera a URL de Autorização
            var authorizationUrl = BuildGoogleAuthorizationUrl(settings.GoogleOpenIdClientId, redirectUri, ProtectState(statePayload));

            // Retorna os dados de seção com o Google
            return new GoogleAuthStartResponse
            {
                SessionCode = session.SessionCode,
                AuthorizationUrl = authorizationUrl,
                ExpiresAt = session.ExpiresAt
            };
        }

        /// <summary>
        /// Obtém status do login com Google
        /// </summary>
        /// <param name="sessionCode">Código da seção</param>
        /// <param name="userAgent">Agente do cliente eTasks</param>
        /// <param name="clientInstanceId">Id do cliente</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<GoogleAuthStatusResponse> GetGoogleLoginStatusAsync(Guid sessionCode, string userAgent, string clientInstanceId, CancellationToken cancellationToken = default)
        {
            // Obtem dados da seção com o Google
            var session = await GetOwnedGoogleSessionAsync(sessionCode, userAgent, clientInstanceId, cancellationToken);

            // Retorna o status da seção com o Google
            return new GoogleAuthStatusResponse
            {
                SessionCode = session.SessionCode,
                Status = session.Status,
                ErrorCode = session.ErrorCode,
                ErrorDescription = session.ErrorDescription,
                ExpiresAt = session.ExpiresAt
            };
        }

        /// <summary>
        /// Consome o Login com o Google, gerando as credenciais de acesso: Token e Refresh Token
        /// </summary>
        /// <param name="request">Dados para consumir google</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<LoginResponse> ConsumeGoogleLoginAsync(GoogleAuthConsumeRequest request, CancellationToken cancellationToken = default)
        {
            // Obtém dados da seção com o Google
            var session = await GetOwnedGoogleSessionAsync(request.SessionCode, request.UserAgent, request.ClientInstanceId, cancellationToken);

            // Verifica se a seção expirou
            if (session.ExpiresAt <= DateTime.UtcNow)
                throw new ApiException(HttpStatusCode.Unauthorized, "A sessão de login com o Google expirou.");

            // Verifica se a conexão / login falhou
            if (session.Status == ExternalAuthSessionStatus.Failed)
                throw new ApiException(HttpStatusCode.Unauthorized, session.ErrorDescription ?? "O login com o Google falhou.");

            // Valida se a seção já foi consumida / usada
            if (session.Status == ExternalAuthSessionStatus.Consumed || session.ConsumedAt is not null)
                throw new ApiException(HttpStatusCode.Unauthorized, "Esta sessão de login com o Google já foi consumida.");

            // Verifica se o processo de login com o Google já foi concluído
            if (session.Status != ExternalAuthSessionStatus.Success || string.IsNullOrWhiteSpace(session.ProtectedLoginResponseJson))
                throw new ApiException(HttpStatusCode.Accepted, "O login com o Google ainda não foi concluído.");

            // Descompacta/desbloqueia os dados sensiveis do usuário (de conexão com servidor do Google)
            var json = _secretProtector.Unprotect(session.ProtectedLoginResponseJson);

            // Gera a entidade de resposta, com Token e Refresh Token
            var response = JsonSerializer.Deserialize<LoginResponse>(json) ?? throw new ApiException(HttpStatusCode.Unauthorized, "Resposta de login com Google inválida.");

            // Marca que sessão foi usada/consumida
            session.Status = ExternalAuthSessionStatus.Consumed;

            // Grava data de consumo da sessão
            session.ConsumedAt = DateTime.UtcNow;

            // Salva no banco de dados
            await _context.SaveChangesAsync(cancellationToken);

            // Retorna o token e Refresh Token
            return response;
        }

        /// <summary>
        /// Completa o login com o Google
        /// </summary>
        /// <param name="code">código recebido pelo callback</param>
        /// <param name="state">state do callback</param>
        /// <param name="error">erro</param>
        /// <param name="errorDescription">descrição de erro</param>
        /// <param name="ipAddress">endereço ip</param>
        /// <param name="requestBaseUri">URL de requisição</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<GoogleAuthCallbackResult> CompleteGoogleLoginAsync(string? code, string? state, string? error, string? errorDescription, string? ipAddress, Uri requestBaseUri, CancellationToken cancellationToken = default)
        {
            // Pega o state e decodifica para obter dados de sessão
            GoogleStatePayload statePayload;
            try
            {
                statePayload = UnprotectState(state);
            }
            catch
            {
                return new GoogleAuthCallbackResult { Success = false, Message = "State inválido recebido do Google." };
            }

            // Obtém a sessão
            var session = await _context.ExternalAuthSessions.FirstOrDefaultAsync(x => x.SessionCode == statePayload.SessionCode && x.Provider == GoogleProvider, cancellationToken);

            // Valida se sessão existe
            if (session is null)
                return new GoogleAuthCallbackResult { Success = false, SessionCode = statePayload.SessionCode, Message = "Sessao Google não localizada." };

            // Valida se sessão não expirou
            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                // Registra falha da sessão
                MarkGoogleSessionFailed(session, "expired_session", "A sessão de login com o Google expirou.");

                // salva dados
                await _context.SaveChangesAsync(cancellationToken);

                // Gera retorno da função
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            // Valida o state recebido
            if (!string.Equals(session.FixedStateCode, statePayload.FixedStateCode, StringComparison.Ordinal)
                || !string.Equals(session.ClientUserAgent, statePayload.UserAgent, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(session.ClientInstanceId, statePayload.ClientInstanceId, StringComparison.Ordinal))
            {
                // Registra falha na sessão
                MarkGoogleSessionFailed(session, "invalid_state", "State não corresponde a sessão iniciada.");

                // salva dados
                await _context.SaveChangesAsync(cancellationToken);

                // Gera resposta
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            // Valida se ocorreram erros
            if (!string.IsNullOrWhiteSpace(error))
            {
                // Registra falha
                MarkGoogleSessionFailed(session, error, errorDescription ?? "O Google recusou ou cancelou a autenticação.");

                // Salva os dados
                await _context.SaveChangesAsync(cancellationToken);

                // Gera a resposta
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            // Valida o código
            if (string.IsNullOrWhiteSpace(code))
            {
                // Registra falha
                MarkGoogleSessionFailed(session, "missing_code", "O Google não retornou o código de autorização.");

                // Salva dados
                await _context.SaveChangesAsync(cancellationToken);

                // Retorna informação
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            try
            {
                // Obtém informações das configurações do servidor
                var settings = await _serverSettingsProvider.GetCurrentAsync(cancellationToken);

                // Valida configurações
                ValidateGoogleSettings(settings);

                // Obtém URL de redirecionamento
                var redirectUri = ResolveGoogleRedirectUri(settings.GoogleOpenIdRedirectUri, requestBaseUri);

                // Realiza troca de código de autenticação por Token 
                var tokenResponse = await ExchangeGoogleCodeAsync(code, settings.GoogleOpenIdClientId, settings.GoogleOpenIdClientSecret, redirectUri, cancellationToken);

                // Valida o token obtido
                var profile = await ValidateGoogleIdTokenAsync(tokenResponse.IdToken, settings.GoogleOpenIdClientId, cancellationToken);

                // Faz Sign in do usuário no Google
                var loginResponse = await SignInGoogleUserAsync(profile, session.ClientUserAgent, ipAddress, cancellationToken);

                // Protege informações de login
                session.ProtectedLoginResponseJson = _secretProtector.Protect(JsonSerializer.Serialize(loginResponse));

                // Registra status da sessão como sucesso
                session.Status = ExternalAuthSessionStatus.Success;

                // Grava data e hora da operação completada
                session.CompletedAt = DateTime.UtcNow;

                // salva dados
                await _context.SaveChangesAsync(cancellationToken);

                // Gera resultado para função callback
                return BuildGoogleCallbackResult(session, statePayload, true, "Login com o Google concluído.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao concluir login Google para sessão {SessionCode}.", session.SessionCode);

                // Registra falha
                MarkGoogleSessionFailed(session, "google_login_failed", ex.Message);

                // salva dados
                await _context.SaveChangesAsync(cancellationToken);

                // Gera resposta de callback
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }
        }
        #endregion

        #region Métodos de gestão da Autenticação e Autorização do usuário logado
        /// <summary>
        /// Método para obter novos Token e RefeshToken
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="ApiException"></exception>
        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // Valida Refresh Token informado
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ValidationException("RefreshToken", "O token de renovação é obrigatório.");

            // Valida o agente do usuário se veio preenchido
            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException("UserAgent", "O UserAgent é obrigatório.");

            // Verifica se Refresh Token existe no banco de dados e se não está revogado
            var tokenRecord = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && !r.IsRevoked);

            // Testa se a sessão não é inválida
            if (tokenRecord == null)
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sessão inválida. Faça login novamente.");

            // Testa se o User Agent é aceito e válido
            if (!string.Equals(tokenRecord.UserAgent, request.UserAgent, StringComparison.OrdinalIgnoreCase))
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sessão inválida para este cliente. Faça login novamente.");

            // Valida se o RefreshToken não expirou
            if (tokenRecord.ExpiresAt <= DateTime.UtcNow)
            {
                // Marca o refesh como expirado
                tokenRecord.IsRevoked = true;

                // Salva
                await _context.SaveChangesAsync();

                // retorna
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sua sessão expirou. Faça login novamente.");
            }

            // Identifica se usuário existe e se não se encontra "deletado"
            if (tokenRecord.User is null || tokenRecord.User.IsDeleted)
            {
                // Revoga token
                tokenRecord.IsRevoked = true;

                // salva
                await _context.SaveChangesAsync();

                // reporta
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi removida e não pode mais ser utilizada.");
            }

            // Valida se conta do usuário não está bloqueada
            if (tokenRecord.User!.IsBlocked)
            {
                _logger.LogWarning("Usuário {Uid} tentou renovar token, mas encontra-se bloqueado.", tokenRecord.User.Uid);

                // Adiciona informação ao log de logins
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = tokenRecord.User.Uid, Status = "Blocked", IpAddress = null, UserAgent = request.UserAgent });

                // Salva
                await _context.SaveChangesAsync();

                // Reporta
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            // revoga o refresh token usado
            tokenRecord.IsRevoked = true;

            // Gera novas credenciais (token e refresh token
            return await GenerateAuthResponseAsync(tokenRecord.User!, request.UserAgent);
        }

        /// <summary>
        /// Envia e-mail paar trocar senha se o usuário a esqueceu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            _logger.LogInformation("Solicitação de recuperação de senha recebida para o e-mail: {Email}", request.Email);

            // Valida se usuário existe e não está deletado
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user == null)
            {
                _logger.LogInformation("O e-mail {Email} não foi localizado na base. Abortando silenciosamente por segurança.", request.Email);
                return true;
            }

            // Gera código aleatório
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            // Registra código de resetamento de senha
            var resetCode = new PasswordResetCode
            {
                UserUid = user.Uid,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            // Grava informação no banco
            await _context.PasswordResetCodes.AddAsync(resetCode);

            // Salva
            await _context.SaveChangesAsync();

            // Envia e-mail de recuperação de senha
            await _emailService.SendPasswordResetEmailAsync(user.Email, code);

            return true;
        }

        /// <summary>
        /// Reseta senha
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Valida se usuário existe e se conta não está excluída
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

            if (user == null) throw new ValidationException("Geral", "Solicitação inválida.");

            // Obtém códigos de resetamento de senha não usados e não expirados
            var resetCode = await _context.PasswordResetCodes
                .Where(c => c.UserUid == user.Uid && c.Code == request.Code && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            // Se não encontrar   
            if (resetCode == null)
                throw new ValidationException("Code", "Código inválido ou já expirado.");

            // Seta código como usado
            resetCode.IsUsed = true;

            // Protege / criptografa senha
            user.PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

            // Resgata todas as sessões do usuário (refresh tokens e tokens ativos) e revoga todos
            var activeTokens = await _context.RefreshTokens.Where(r => r.UserUid == user.Uid && !r.IsRevoked).ToListAsync();
            foreach (var tk in activeTokens) tk.IsRevoked = true;

            // salva dados
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Trocar senha
        /// </summary>
        /// <param name="userUid">UID do usuário</param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<bool> ChangePasswordAsync(Guid userUid, ChangePasswordRequest request)
        {
            _logger.LogInformation("Solicitação de mudançaa de senha para o usuário: {Uid}", userUid);

            // Valida se usuário existe e não se encontra excluído
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == userUid && !u.IsDeleted);

            if (user == null) throw new ValidationException("User", "Usuário não localizado.");

            // Valida a senha atual
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, _secretProtector.Unprotect(user.PasswordHash)))
            {
                _logger.LogWarning("O usuário {Uid} tentou trocar a senha, mas forneceu a senha atual incorretamente.", userUid);
                throw new ValidationException("CurrentPassword", "A senha atual está incorreta.");
            }

            // Criptografa a nova senha fornecida
            user.PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

            // Revoga todas as sessões ativas
            var activeTokens = await _context.RefreshTokens.Where(r => r.UserUid == user.Uid && !r.IsRevoked).ToListAsync();
            foreach (var tk in activeTokens) tk.IsRevoked = true;

            // Salva
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Marca conta como e-mail confirmado
        /// </summary>
        /// <param name="token">token de validação</param>
        /// <returns></returns>
        public async Task<bool> ConfirmEmailAsync(string token)
        {
            try
            {
                // Gera o token para comparação e valida
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration[Constants.JwtKeyConfig] ?? "default_very_secret_key_1234567890_min_32_chars!");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var uidClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "ConfirmEmail")?.Value;

                // Verifica se o usuário do token é o mesmo logado
                if (string.IsNullOrEmpty(uidClaim) || !Guid.TryParse(uidClaim, out Guid userUid))
                    return false;

                // Verifica se usuário existe e se não se encontra excluído
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == userUid && !u.IsDeleted);
                if (user == null || user.IsConfirmed) return true;

                // Confirma e-mail
                user.IsConfirmed = true;

                // Salva
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha na validação do token de e-mail.");
                return false;
            }
        }

        /// <summary>
        /// Recupera conta apagada
        /// </summary>
        /// <param name="code">código recebido para recuperação</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AccountRecoveryResult> RecoverDeletedAccountAsync(string code, CancellationToken cancellationToken = default)
        {
            // Valida se foi fornecido o código
            if (string.IsNullOrWhiteSpace(code))
            {
                // Apaga contas expiradas
                await _accountDeletionRetentionService.DeleteExpiredAccountsAsync(cancellationToken);

                // Retorna resposta a solicitação
                return new AccountRecoveryResult
                {
                    Success = false,
                    Message = "O link de recuperação informado é inválido."
                };
            }

            // Obtém dados da recuperação e valida código informado, incluíndo se já não foi usado
            var reactivationCode = await _context.AccountReactivationCodes
                .Include(x => x.User)
                .Where(x => x.Code == code.Trim() && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            // Valida se conta ainda existe
            if (reactivationCode?.User is null)
            {
                // Remove contas expiradas
                await _accountDeletionRetentionService.DeleteExpiredAccountsAsync(cancellationToken);

                // Dá retorno a solicitação
                return new AccountRecoveryResult
                {
                    Success = false,
                    Message = "O link de recuperação informado é inválido."
                };
            }

            // Valida se código não se encontra expirado
            if (reactivationCode.ExpiresAt <= DateTime.UtcNow)
            {
                var userUid = reactivationCode.User.Uid;

                // Apaga comnta do usuário
                _context.Users.Remove(reactivationCode.User);

                // salva
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Usuário {Uid} removido permanentemente após expiração do link de recuperacao.", userUid);

                // Dá retorno
                return new AccountRecoveryResult
                {
                    Success = false,
                    Expired = true,
                    Message = "O prazo para recuperar sua conta foi excedido. A conta foi permanentemente excluída."
                };
            }

            // Desmarca conta de ser excluída e remove bloqueios também
            var user = reactivationCode.User;
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.IsBlocked = false;
            user.LastAccessAt = SaoPauloDateTime.Now();
            reactivationCode.IsUsed = true;
            reactivationCode.UsedAt = DateTime.UtcNow;

            // salva
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Conta do usuário {Uid} reativada por link de recuperação.", user.Uid);

            // Gera retorno
            return new AccountRecoveryResult
            {
                Success = true,
                Message = "Sua conta foi recuperada com sucesso. Você já pode acessar o eTasks novamente."
            };
        }

        /// <summary>
        /// Função para revogar a validade de um refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token a revogar</param>
        /// <returns></returns>
        public async Task RevokeRefreshTokenAsync(string? refreshToken)
        {
            // Valida de Refresh Token informado está em branco ou vazio
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            // Obtém dados do refresh token
            var tokenRecord = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked);

            // Valida se o mesmo não foi encontrado
            if (tokenRecord is null)
            {
                return;
            }

            // Seta o refresh token como revogado
            tokenRecord.IsRevoked = true;

            // Salva
            await _context.SaveChangesAsync();
        }
        #endregion

        #endregion

        #region Métodos privados/particulares da classe
        /// <summary>
        /// Gera a resposta com os Token e RefreshToken
        /// </summary>
        /// <param name="user">Usuário</param>
        /// <param name="userAgent">User Agent do cliente</param>
        /// <returns></returns>
        private async Task<LoginResponse> GenerateAuthResponseAsync(User user, string? userAgent)
        {
            // Gera o Token principal (Access Token)
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKeyString = _configuration[Constants.JwtKeyConfig] ?? "default_very_secret_key_1234567890_min_32_chars!";
            var key = Encoding.UTF8.GetBytes(jwtKeyString);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(Constants.UserAgentClaimType, userAgent ?? string.Empty)
            };

            // Adiciona a Claim se o usuário é Administrador ou usuário final
            claims.Add(new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"));

            // Seta data/hora de expiração (4 horas adiante)
            var jwtExpiration = DateTime.UtcNow.AddHours(4);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = jwtExpiration,
                Issuer = _configuration[Constants.JwtIssuerConfig],
                Audience = _configuration[Constants.JwtAudienceConfig],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Grava o Token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // Define o refesh Token usando combinação de números aleatórios
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            // Seta o tempo de expiração do Refresh Token (30 dias de validade)
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(30);

            var refreshToken = new RefreshToken
            {
                UserUid = user.Uid,
                Token = refreshTokenString,
                UserAgent = userAgent,
                ExpiresAt = refreshTokenExpiration
            };

            // Salvar dados do Refresh Token
            await _context.RefreshTokens.AddAsync(refreshToken);

            // Grava data de último acesso
            user.LastAccessAt = SaoPauloDateTime.Now();

            // Salva dados
            await _context.SaveChangesAsync();

            // Gera a entidade de resposta
            return new LoginResponse
            {
                Token = jwtToken,
                TokenExpiresAt = jwtExpiration,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiresAt = refreshTokenExpiration
            };
        }

        /// <summary>
        /// Método que retorna o link para confirmação da conta de usuário criada, com validade de 1 dia
        /// </summary>
        /// <param name="UserUID">UID do usuário novo</param>
        /// <returns></returns>
        private string GetConfirmationLink(string UserUID)
        {
            // Gerador de Token JWT do novo usuário para confirmar conta via e-mail
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration[Constants.JwtKeyConfig] ?? "defaultSecretKey_1234567890_min32chars!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("ConfirmEmail", UserUID) }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Grava token de confirmação de e-mail
            var confirmationToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            // Gera link para confirmação de e-amil
            var baseUrl = _configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
            return $"{(baseUrl + _configuration[Constants.ApiV2Path]).TrimEnd('/')}/auth/confirm-email?token={confirmationToken}";
        }

        /// <summary>
        /// Loga com usuário usando conta Google
        /// </summary>
        /// <param name="profile">Perfil obtido do Google</param>
        /// <param name="userAgent">User Agent do cliente</param>
        /// <param name="ipAddress">Endereço IP</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        private async Task<LoginResponse> SignInGoogleUserAsync(GoogleTokenInfo profile, string userAgent, string? ipAddress, CancellationToken cancellationToken)
        {
            // Valida se o e-mail do usuário Google não é verificado
            if (!profile.IsEmailVerified)
                throw new ApiException(HttpStatusCode.Unauthorized, "O e-mail Google precisa estar verificado.");

            // Obtem dados do login externo se existir
            var externalLogin = await _context.UserExternalLogins
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Provider == GoogleProvider && x.ProviderUserId == profile.Sub, cancellationToken);

            // Obtém usuário (dados)
            var user = externalLogin?.User;

            // Se usuário não existir
            if (user is null)
            {
                // Valida se usuário existe
                user = await _context.Users.FirstOrDefaultAsync(x => x.Email == profile.Email, cancellationToken);
                if (user is null)
                {
                    user = new User
                    {
                        Name = NormalizeGoogleName(profile.Name, profile.Email),
                        Email = profile.Email,
                        PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))),
                        PhotoPath = profile.Picture,
                        IsAdmin = false,
                        IsConfirmed = true
                    };

                    // Adiciona usuário a base
                    await _context.Users.AddAsync(user, cancellationToken);

                    // Adiciona configurações
                    await _context.UserSettings.AddAsync(new UserSettings { UserUid = user.Uid }, cancellationToken);
                }

                // Cria perfil externo
                externalLogin = new UserExternalLogin
                {
                    UserUid = user.Uid,
                    Provider = GoogleProvider,
                    ProviderUserId = profile.Sub,
                    Email = profile.Email,
                    DisplayName = profile.Name
                };

                // Adiciona os dados a salvar
                await _context.UserExternalLogins.AddAsync(externalLogin, cancellationToken);
            }
            else
            {
                // Preenche dados do login externo
                var existingExternalLogin = externalLogin!;
                existingExternalLogin.Email = profile.Email;
                existingExternalLogin.DisplayName = profile.Name;
                existingExternalLogin.UpdatedAt = SaoPauloDateTime.Now();
            }

            // Valida se usuário não se encontra deletado
            if (user.IsDeleted)
                throw new ApiException(HttpStatusCode.Forbidden, "Sua conta foi removida e não pode mais ser utilizada.");

            // Valida se a conta não está bloqueada
            if (user.IsBlocked)
                throw new ApiException(HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");

            // Confirma a conta
            user.IsConfirmed = true;

            // Registra data do último acesso
            user.LastAccessAt = SaoPauloDateTime.Now();

            // Salva dados do log de logins
            await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Success", IpAddress = ipAddress, UserAgent = userAgent }, cancellationToken);

            // Gera a resposta de autenticação e autorização
            return await GenerateAuthResponseAsync(user, userAgent);
        }

        /// <summary>
        /// Obtem dados da seção Google
        /// </summary>
        /// <param name="sessionCode">Código da sessão</param>
        /// <param name="userAgent">Agente do usuário do cliente</param>
        /// <param name="clientInstanceId">Id da instânica do cliente</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="ApiException"></exception>
        private async Task<ExternalAuthSession> GetOwnedGoogleSessionAsync(Guid sessionCode, string userAgent, string clientInstanceId, CancellationToken cancellationToken)
        {
            // Valida se foi enviado código da sessão
            if (sessionCode == Guid.Empty)
                throw new ValidationException(nameof(sessionCode), "Informe o código da sessão do Google.");

            // Tenta obter dados da sessão com Google
            var session = await _context.ExternalAuthSessions.FirstOrDefaultAsync(x => x.SessionCode == sessionCode && x.Provider == GoogleProvider, cancellationToken);

            // Se não encontrar sessão
            if (session is null)
                throw new ApiException(HttpStatusCode.NotFound, "Sessao de login com Google não localizada.");

            // Valida se a sessão pertence ao mesmo cliente informado
            if (!string.Equals(session.ClientUserAgent, userAgent, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(session.ClientInstanceId, clientInstanceId, StringComparison.Ordinal))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "Sessão Google não pertence a este cliente.");
            }

            // Retorna dados da sessão
            return session;
        }

        /// <summary>
        /// Valida configurações de Login com Provedor OAuth Google
        /// </summary>
        /// <param name="settings">Configurações do servidor</param>
        /// <exception cref="ApiException"></exception>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateGoogleSettings(global::eTasks_server.Models.Entities.Settings.ServerSettings settings)
        {
            // Verifica se Login com Google está habilitado
            if (!settings.GoogleOpenIdEnabled)
                throw new ApiException(HttpStatusCode.ServiceUnavailable, "Login com Google está desabilitado no servidor.");

            // Verifica se o ClientID foi informado
            if (string.IsNullOrWhiteSpace(settings.GoogleOpenIdClientId))
                throw new ValidationException(nameof(settings.GoogleOpenIdClientId), "Configure o Client ID do Google.");

            // Verifica se o ClientSecret foi informado
            if (string.IsNullOrWhiteSpace(settings.GoogleOpenIdClientSecret))
                throw new ValidationException(nameof(settings.GoogleOpenIdClientSecret), "Configure o Client Secret do Google.");

            // Verifica se o Código State Fixo da aplicação foi informado
            if (string.IsNullOrWhiteSpace(settings.GoogleOpenIdStateCode))
                throw new ValidationException(nameof(settings.GoogleOpenIdStateCode), "Configure o código fixo de state do Google.");
        }

        /// <summary>
        /// Decide qual será a URL a redirecionar
        /// </summary>
        /// <param name="configuredRedirectUri">URL a redirecionar</param>
        /// <param name="requestBaseUri">URL da Requisição</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        private static string ResolveGoogleRedirectUri(string configuredRedirectUri, Uri? requestBaseUri)
        {
            // Se a URL de redirecionamento estiver vazia ou em branco
            if (!string.IsNullOrWhiteSpace(configuredRedirectUri))
                return configuredRedirectUri.Trim();

            // Valida se a URL base da requisição existe
            if (requestBaseUri is null)
                throw new ValidationException(nameof(configuredRedirectUri), "Configure a Redirect URI do Google.");

            // Retorna a URL de redirecionamento com o Google (método Callback)
            return new Uri(requestBaseUri, "api/v2/auth/google/callback").ToString();
        }

        /// <summary>
        /// Constroi URL de Autorização com Google
        /// </summary>
        /// <param name="clientId">ID do cliente</param>
        /// <param name="redirectUri">URL de redirecionamento</param>
        /// <param name="state">Código State</param>
        /// <returns></returns>
        private static string BuildGoogleAuthorizationUrl(string clientId, string redirectUri, string state)
        {
            // Monta parâmetros de query da URL
            var query = new Dictionary<string, string?>
            {
                ["client_id"] = clientId,
                ["redirect_uri"] = redirectUri,
                ["response_type"] = "code",
                ["scope"] = "openid email profile",
                ["state"] = state,
                ["access_type"] = "offline",
                ["prompt"] = "select_account"
            };

            // Retorna URL de Autenticação / autorização com o Google
            return "https://accounts.google.com/o/oauth2/v2/auth?" + string.Join("&", query.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
        }

        /// <summary>
        /// Troca o Código de acesso por credenciais do Google
        /// </summary>
        /// <param name="code">Código de acesso</param>
        /// <param name="clientId">ClientID</param>
        /// <param name="clientSecret">ClientSecret</param>
        /// <param name="redirectUri">URL de redirecionamento</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        private async Task<GoogleTokenResponse> ExchangeGoogleCodeAsync(string code, string clientId, string clientSecret, string redirectUri, CancellationToken cancellationToken)
        {
            // Cria serviço HTTP
            using var client = _httpClientFactory.CreateClient();

            // Envia requisição para API Google de obtenção de credenciais de autenticação / autorização
            using var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            }), cancellationToken);

            // Valida se resposta foi mal sucedida
            if (!response.IsSuccessStatusCode)
                throw new ApiException(HttpStatusCode.Unauthorized, "Não foi possível trocar o código Google por tokens.");

            // Lê / Obtem o token da resposta da requisição
            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);

            // Se token for vazio ou em branco
            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.IdToken))
                throw new ApiException(HttpStatusCode.Unauthorized, "Resposta de token Google inválida.");

            // Retorna o Token
            return tokenResponse;
        }

        /// <summary>
        /// Valida o Token do Google
        /// </summary>
        /// <param name="idToken">Token</param>
        /// <param name="clientId">ClientId</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        private async Task<GoogleTokenInfo> ValidateGoogleIdTokenAsync(string idToken, string clientId, CancellationToken cancellationToken)
        {
            // Cria serviço HTTP para realizar requisição
            using var client = _httpClientFactory.CreateClient();

            // Obtém informações do usuário no Google
            var tokenInfo = await client.GetFromJsonAsync<GoogleTokenInfo>($"https://oauth2.googleapis.com/tokeninfo?id_token={WebUtility.UrlEncode(idToken)}", cancellationToken);
            
            // Se retornar vazio
            if (tokenInfo is null)
                throw new ApiException(HttpStatusCode.Unauthorized, "ID token Google inválido.");

            // Compara a audiência com o ClientID
            if (!string.Equals(tokenInfo.Aud, clientId, StringComparison.Ordinal))
                throw new ApiException(HttpStatusCode.Unauthorized, "ID token Google emitido para outro Client ID.");

            // Verifica o emissor da resposta se foi o Google
            if (!string.Equals(tokenInfo.Iss, "https://accounts.google.com", StringComparison.Ordinal)
                && !string.Equals(tokenInfo.Iss, "accounts.google.com", StringComparison.Ordinal))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "Emissor do ID token Google inválido.");
            }

            // Valida se foram obtidas as informações do usuário
            if (string.IsNullOrWhiteSpace(tokenInfo.Sub) || string.IsNullOrWhiteSpace(tokenInfo.Email))
                throw new ApiException(HttpStatusCode.Unauthorized, "Perfil Google incompleto.");

            // Retorna os dados da Conta Google
            return tokenInfo;
        }

        /// <summary>
        /// Método para proteger/criptografar o state
        /// </summary>
        /// <param name="payload">Carga do state</param>
        /// <returns></returns>
        private string ProtectState(GoogleStatePayload payload)
        {
            // Serializa o payload para gerar um state robusto
            var protectedJson = _secretProtector.Protect(JsonSerializer.Serialize(payload));
            return Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(protectedJson));
        }

        /// <summary>
        /// Desproteger/Descriptografar o State
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        private GoogleStatePayload UnprotectState(string? state)
        {
            // Valida se State está vazio ou em branco
            if (string.IsNullOrWhiteSpace(state))
                throw new ValidationException("state", "State é obrigatório.");

            // Descriptografa o state
            var protectedJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(state));
            var json = _secretProtector.Unprotect(protectedJson);
            return JsonSerializer.Deserialize<GoogleStatePayload>(json) ?? throw new ValidationException("state", "State é inválido.");
        }

        /// <summary>
        /// Constroe resposta do método callback do Google
        /// </summary>
        /// <param name="session">sessão</param>
        /// <param name="statePayload">state</param>
        /// <param name="success">sucesso</param>
        /// <param name="message">mensagem</param>
        /// <returns></returns>
        private GoogleAuthCallbackResult BuildGoogleCallbackResult(ExternalAuthSession session, GoogleStatePayload statePayload, bool success, string message)
        {
            // Obtem a URL de redirecionamento
            var redirectUrl = BuildClientRedirectUrl(statePayload.ReturnUrl, session.SessionCode, success, session.ErrorCode);

            // Monta o retorno do Callback
            return new GoogleAuthCallbackResult
            {
                Success = success,
                SessionCode = session.SessionCode,
                UserAgent = session.ClientUserAgent,
                RedirectUrl = redirectUrl,
                Message = message
            };
        }

        /// <summary>
        /// Gera o redirecionamento do Cliente
        /// </summary>
        /// <param name="returnUrl">URL retornada</param>
        /// <param name="sessionCode">Código da sessão</param>
        /// <param name="success">Sucesso</param>
        /// <param name="errorCode">Código de erro</param>
        /// <returns></returns>
        private static string? BuildClientRedirectUrl(string? returnUrl, Guid sessionCode, bool success, string? errorCode)
        {
            // Valida se URL de retorno está vazio ou em branco
            if (string.IsNullOrWhiteSpace(returnUrl))
                return null;
            
            // Tenta recriar URL de retorno
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
                return null;

            // Obtem o separador
            var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";

            // Adiciona os parâmetros
            var url = $"{uri}{separator}googleSession={WebUtility.UrlEncode(sessionCode.ToString())}&success={success.ToString().ToLowerInvariant()}";

            // Se foi passado código de erro, repassa na URL
            if (!string.IsNullOrWhiteSpace(errorCode))
                url += $"&error={WebUtility.UrlEncode(errorCode)}";

            return url;
        }

        /// <summary>
        /// Marcar a sessão com Google como falha
        /// </summary>
        /// <param name="session">sessão</param>
        /// <param name="errorCode">código de erro</param>
        /// <param name="errorDescription">Descrição do erro</param>
        private static void MarkGoogleSessionFailed(ExternalAuthSession session, string errorCode, string errorDescription)
        {           
            session.Status = ExternalAuthSessionStatus.Failed;
            session.ErrorCode = errorCode;
            session.ErrorDescription = errorDescription;
            session.CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Normaliza Nome da Conta Google retornado
        /// </summary>
        /// <param name="name">Nome</param>
        /// <param name="email">E-mail</param>
        /// <returns></returns>
        private static string NormalizeGoogleName(string? name, string email)
        {
            // Se o nome vier vazio ou em branco, pega o nome do inicio do e-mail antes do @, se não pega o nome retornado
            var value = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name.Trim();

            // Se nome retornado for menor que 3 caracteres, adiciona _ ao nome para completar pelo menos 3 caracteres
            if (value.Length < 3)
                value = value.PadRight(3, '_');

            // Retorna os 30 primeiros dígitos/caracteres do nome
            return value.Length > 30 ? value[..30] : value;
        }
        #endregion
    }
}
