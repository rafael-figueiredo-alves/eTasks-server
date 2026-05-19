using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Helpers;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.DTOs.Auth.Responses;
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
    public class AuthBLL : BaseBLL<IAuthBLL>, IAuthBLL
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISecretProtector _secretProtector;
        private readonly IServerSettingsProvider _serverSettingsProvider;
        private readonly IAccountDeletionRetentionService _accountDeletionRetentionService;
        private readonly IHttpClientFactory _httpClientFactory;

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

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Iniciando processo de registro para o e-mail: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Geral", "E-mail e senha sao obrigatorios.");

            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException("UserAgent", "O UserAgent e obrigatorio.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Tentativa de registro falhou. O e-mail {Email} ja esta em uso.", request.Email);
                throw new ValidationException("Email", "O e-mail informado ja esta cadastrado.");
            }

            string? photoPath = null;
            if (!string.IsNullOrWhiteSpace(request.PhotoBase64))
            {
                try
                {
                    photoPath = await UserPhotoStorage.SaveAsync(request.PhotoBase64, null);
                }
                catch
                {
                }
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.Password)),
                PhotoPath = photoPath,
                IsAdmin = false
            };

            await _context.Users.AddAsync(user);
            await _context.UserSettings.AddAsync(new UserSettings { UserUid = user.Uid });
            await _context.SaveChangesAsync();
            _logger.LogInformation("Novo usuario criado no banco de dados. Uid: {Uid}, E-mail: {Email}", user.Uid, user.Email);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration[Constants.JwtKeyConfig] ?? "defaultSecretKey_1234567890_min32chars!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("ConfirmEmail", user.Uid.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var confirmationToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            var baseUrl = _configuration[Constants.ApiBaseUrl] ?? "http://localhost:5033";
            var confirmationLink = $"{(baseUrl + _configuration[Constants.ApiV2Path]).TrimEnd('/')}/auth/confirm-email?token={confirmationToken}";

#pragma warning disable CS4014
            _emailService.SendAccountConfirmationEmailAsync(user.Email, confirmationLink);
#pragma warning restore CS4014

            return await GenerateAuthResponseAsync(user, request.UserAgent);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
        {
            _logger.LogInformation("Iniciando tentativa de login para o usuario: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Geral", "Forneca o e-mail e a senha para continuar.");

            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException("UserAgent", "O UserAgent e obrigatorio.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning("Falha de autenticacao. E-mail nao encontrado: {Email}", request.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = null, Status = "Failed", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Nao encontramos uma conta com esse e-mail.");
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Usuario {Uid} ({Email}) tentou logar, mas a conta foi removida.", user.Uid, user.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Blocked", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi removida e nao pode mais ser utilizada.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, _secretProtector.Unprotect(user.PasswordHash)))
            {
                _logger.LogWarning("Falha de autenticacao. Senha incorreta para o usuario: {Uid}", user.Uid);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Failed", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Senha incorreta. Verifique e tente novamente.");
            }

            if (user.IsBlocked)
            {
                _logger.LogWarning("Usuario {Uid} ({Email}) tentou logar, mas encontra-se bloqueado.", user.Uid, user.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Blocked", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            _logger.LogInformation("Usuario {Uid} autenticado com sucesso.", user.Uid);

            await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Success", IpAddress = ipAddress, UserAgent = request.UserAgent });
            user.LastAccessAt = SaoPauloDateTime.Now();

            return await GenerateAuthResponseAsync(user, request.UserAgent);
        }

        public async Task<GoogleAuthStartResponse> StartGoogleLoginAsync(GoogleAuthStartRequest request, Uri requestBaseUri, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException(nameof(request.UserAgent), "O UserAgent e obrigatorio.");

            if (!Constants.ApiClientUserAgents.Contains(request.UserAgent, StringComparer.OrdinalIgnoreCase))
                throw new ValidationException(nameof(request.UserAgent), "O login Google e permitido apenas para clientes da API.");

            if (string.IsNullOrWhiteSpace(request.ClientInstanceId))
                throw new ValidationException(nameof(request.ClientInstanceId), "O identificador do cliente e obrigatorio.");

            var settings = await _serverSettingsProvider.GetCurrentAsync(cancellationToken);
            ValidateGoogleSettings(settings);

            var session = new ExternalAuthSession
            {
                Provider = GoogleProvider,
                ClientUserAgent = request.UserAgent.Trim().ToLowerInvariant(),
                ClientInstanceId = request.ClientInstanceId.Trim(),
                FixedStateCode = settings.GoogleOpenIdStateCode.Trim(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            await _context.ExternalAuthSessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var redirectUri = ResolveGoogleRedirectUri(settings.GoogleOpenIdRedirectUri, requestBaseUri);
            var statePayload = new GoogleStatePayload
            {
                FixedStateCode = session.FixedStateCode,
                SessionCode = session.SessionCode,
                UserAgent = session.ClientUserAgent,
                ClientInstanceId = session.ClientInstanceId,
                ReturnUrl = request.ReturnUrl,
                CreatedAtUtc = DateTime.UtcNow
            };

            var authorizationUrl = BuildGoogleAuthorizationUrl(settings.GoogleOpenIdClientId, redirectUri, ProtectState(statePayload));

            return new GoogleAuthStartResponse
            {
                SessionCode = session.SessionCode,
                AuthorizationUrl = authorizationUrl,
                ExpiresAt = session.ExpiresAt
            };
        }

        public async Task<GoogleAuthStatusResponse> GetGoogleLoginStatusAsync(Guid sessionCode, string userAgent, string clientInstanceId, CancellationToken cancellationToken = default)
        {
            var session = await GetOwnedGoogleSessionAsync(sessionCode, userAgent, clientInstanceId, cancellationToken);
            return new GoogleAuthStatusResponse
            {
                SessionCode = session.SessionCode,
                Status = session.Status,
                ErrorCode = session.ErrorCode,
                ErrorDescription = session.ErrorDescription,
                ExpiresAt = session.ExpiresAt
            };
        }

        public async Task<LoginResponse> ConsumeGoogleLoginAsync(GoogleAuthConsumeRequest request, CancellationToken cancellationToken = default)
        {
            var session = await GetOwnedGoogleSessionAsync(request.SessionCode, request.UserAgent, request.ClientInstanceId, cancellationToken);
            if (session.ExpiresAt <= DateTime.UtcNow)
                throw new ApiException(HttpStatusCode.Unauthorized, "A sessao de login Google expirou.");

            if (session.Status == ExternalAuthSessionStatus.Failed)
                throw new ApiException(HttpStatusCode.Unauthorized, session.ErrorDescription ?? "O login Google falhou.");

            if (session.Status == ExternalAuthSessionStatus.Consumed || session.ConsumedAt is not null)
                throw new ApiException(HttpStatusCode.Unauthorized, "Esta sessao de login Google ja foi consumida.");

            if (session.Status != ExternalAuthSessionStatus.Success || string.IsNullOrWhiteSpace(session.ProtectedLoginResponseJson))
                throw new ApiException(HttpStatusCode.Accepted, "O login Google ainda nao foi concluido.");

            var json = _secretProtector.Unprotect(session.ProtectedLoginResponseJson);
            var response = JsonSerializer.Deserialize<LoginResponse>(json) ?? throw new ApiException(HttpStatusCode.Unauthorized, "Resposta de login Google invalida.");

            session.Status = ExternalAuthSessionStatus.Consumed;
            session.ConsumedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }

        public async Task<GoogleAuthCallbackResult> CompleteGoogleLoginAsync(string? code, string? state, string? error, string? errorDescription, string? ipAddress, Uri requestBaseUri, CancellationToken cancellationToken = default)
        {
            GoogleStatePayload statePayload;
            try
            {
                statePayload = UnprotectState(state);
            }
            catch
            {
                return new GoogleAuthCallbackResult { Success = false, Message = "State invalido recebido do Google." };
            }

            var session = await _context.ExternalAuthSessions.FirstOrDefaultAsync(x => x.SessionCode == statePayload.SessionCode && x.Provider == GoogleProvider, cancellationToken);
            if (session is null)
                return new GoogleAuthCallbackResult { Success = false, SessionCode = statePayload.SessionCode, Message = "Sessao Google nao localizada." };

            if (session.ExpiresAt <= DateTime.UtcNow)
            {
                MarkGoogleSessionFailed(session, "expired_session", "A sessao de login Google expirou.");
                await _context.SaveChangesAsync(cancellationToken);
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            if (!string.Equals(session.FixedStateCode, statePayload.FixedStateCode, StringComparison.Ordinal)
                || !string.Equals(session.ClientUserAgent, statePayload.UserAgent, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(session.ClientInstanceId, statePayload.ClientInstanceId, StringComparison.Ordinal))
            {
                MarkGoogleSessionFailed(session, "invalid_state", "State nao corresponde a sessao iniciada.");
                await _context.SaveChangesAsync(cancellationToken);
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                MarkGoogleSessionFailed(session, error, errorDescription ?? "O Google recusou ou cancelou a autenticacao.");
                await _context.SaveChangesAsync(cancellationToken);
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                MarkGoogleSessionFailed(session, "missing_code", "O Google nao retornou o codigo de autorizacao.");
                await _context.SaveChangesAsync(cancellationToken);
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }

            try
            {
                var settings = await _serverSettingsProvider.GetCurrentAsync(cancellationToken);
                ValidateGoogleSettings(settings);
                var redirectUri = ResolveGoogleRedirectUri(settings.GoogleOpenIdRedirectUri, requestBaseUri);
                var tokenResponse = await ExchangeGoogleCodeAsync(code, settings.GoogleOpenIdClientId, settings.GoogleOpenIdClientSecret, redirectUri, cancellationToken);
                var profile = await ValidateGoogleIdTokenAsync(tokenResponse.IdToken, settings.GoogleOpenIdClientId, cancellationToken);

                var loginResponse = await SignInGoogleUserAsync(profile, session.ClientUserAgent, ipAddress, cancellationToken);
                session.ProtectedLoginResponseJson = _secretProtector.Protect(JsonSerializer.Serialize(loginResponse));
                session.Status = ExternalAuthSessionStatus.Success;
                session.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                return BuildGoogleCallbackResult(session, statePayload, true, "Login Google concluido.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao concluir login Google para sessao {SessionCode}.", session.SessionCode);
                MarkGoogleSessionFailed(session, "google_login_failed", ex.Message);
                await _context.SaveChangesAsync(cancellationToken);
                return BuildGoogleCallbackResult(session, statePayload, false, session.ErrorDescription!);
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ValidationException("RefreshToken", "O token de renovacao e obrigatorio.");

            if (string.IsNullOrWhiteSpace(request.UserAgent))
                throw new ValidationException("UserAgent", "O UserAgent e obrigatorio.");

            var tokenRecord = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && !r.IsRevoked);

            if (tokenRecord == null)
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sessao invalida. Faca login novamente.");

            if (!string.Equals(tokenRecord.UserAgent, request.UserAgent, StringComparison.OrdinalIgnoreCase))
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sessao invalida para este cliente. Faca login novamente.");

            if (tokenRecord.ExpiresAt <= DateTime.UtcNow)
            {
                tokenRecord.IsRevoked = true;
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sua sessao expirou. Faca login novamente.");
            }

            if (tokenRecord.User is null || tokenRecord.User.IsDeleted)
            {
                tokenRecord.IsRevoked = true;
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi removida e nao pode mais ser utilizada.");
            }

            if (tokenRecord.User!.IsBlocked)
            {
                _logger.LogWarning("Usuario {Uid} tentou renovar token, mas encontra-se bloqueado.", tokenRecord.User.Uid);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = tokenRecord.User.Uid, Status = "Blocked", IpAddress = null, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            tokenRecord.IsRevoked = true;

            return await GenerateAuthResponseAsync(tokenRecord.User!, request.UserAgent);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            _logger.LogInformation("Solicitacao de recuperacao de senha recebida para o e-mail: {Email}", request.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (user == null)
            {
                _logger.LogInformation("O e-mail {Email} nao foi localizado na base. Abortando silenciosamente por seguranca.", request.Email);
                return true;
            }

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            var resetCode = new PasswordResetCode
            {
                UserUid = user.Uid,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _context.PasswordResetCodes.AddAsync(resetCode);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, code);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);
            if (user == null) throw new ValidationException("Geral", "Solicitacao invalida.");

            var resetCode = await _context.PasswordResetCodes
                .Where(c => c.UserUid == user.Uid && c.Code == request.Code && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetCode == null)
                throw new ValidationException("Code", "Codigo invalido ou ja expirado.");

            resetCode.IsUsed = true;
            user.PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

            var activeTokens = await _context.RefreshTokens.Where(r => r.UserUid == user.Uid && !r.IsRevoked).ToListAsync();
            foreach (var tk in activeTokens) tk.IsRevoked = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userUid, ChangePasswordRequest request)
        {
            _logger.LogInformation("Solicitacao de mudanca de senha para o usuario: {Uid}", userUid);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == userUid && !u.IsDeleted);
            if (user == null) throw new ValidationException("User", "Usuario nao localizado.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, _secretProtector.Unprotect(user.PasswordHash)))
            {
                _logger.LogWarning("O usuario {Uid} tentou trocar a senha mas forneceu a senha atual incorretamente.", userUid);
                throw new ValidationException("CurrentPassword", "A senha atual esta incorreta.");
            }

            user.PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

            var activeTokens = await _context.RefreshTokens.Where(r => r.UserUid == user.Uid && !r.IsRevoked).ToListAsync();
            foreach (var tk in activeTokens) tk.IsRevoked = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            try
            {
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

                if (string.IsNullOrEmpty(uidClaim) || !Guid.TryParse(uidClaim, out Guid userUid))
                    return false;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == userUid && !u.IsDeleted);
                if (user == null || user.IsConfirmed) return true;

                user.IsConfirmed = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha na validacao do token de e-mail.");
                return false;
            }
        }

        public async Task<AccountRecoveryResult> RecoverDeletedAccountAsync(string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                await _accountDeletionRetentionService.DeleteExpiredAccountsAsync(cancellationToken);
                return new AccountRecoveryResult
                {
                    Success = false,
                    Message = "O link de recuperacao informado e invalido."
                };
            }

            var reactivationCode = await _context.AccountReactivationCodes
                .Include(x => x.User)
                .Where(x => x.Code == code.Trim() && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (reactivationCode?.User is null)
            {
                await _accountDeletionRetentionService.DeleteExpiredAccountsAsync(cancellationToken);
                return new AccountRecoveryResult
                {
                    Success = false,
                    Message = "O link de recuperacao informado e invalido."
                };
            }

            if (reactivationCode.ExpiresAt <= DateTime.UtcNow)
            {
                var userUid = reactivationCode.User.Uid;
                _context.Users.Remove(reactivationCode.User);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Usuario {Uid} removido permanentemente apos expiracao do link de recuperacao.", userUid);

                return new AccountRecoveryResult
                {
                    Success = false,
                    Expired = true,
                    Message = "O prazo para recuperar sua conta foi excedido. A conta foi permanentemente excluida."
                };
            }

            var user = reactivationCode.User;
            user.IsDeleted = false;
            user.DeletedAt = null;
            user.IsBlocked = false;
            user.LastAccessAt = SaoPauloDateTime.Now();
            reactivationCode.IsUsed = true;
            reactivationCode.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Conta do usuario {Uid} reativada por link de recuperacao.", user.Uid);

            return new AccountRecoveryResult
            {
                Success = true,
                Message = "Sua conta foi recuperada com sucesso. Voce ja pode acessar o eTasks novamente."
            };
        }

        public async Task RevokeRefreshTokenAsync(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var tokenRecord = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked);
            if (tokenRecord is null)
            {
                return;
            }

            tokenRecord.IsRevoked = true;
            await _context.SaveChangesAsync();
        }

        private async Task<LoginResponse> GenerateAuthResponseAsync(User user, string? userAgent)
        {
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

            claims.Add(new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"));

            var jwtExpiration = DateTime.UtcNow.AddHours(4);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = jwtExpiration,
                Issuer = _configuration[Constants.JwtIssuerConfig],
                Audience = _configuration[Constants.JwtAudienceConfig],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(30);

            var refreshToken = new RefreshToken
            {
                UserUid = user.Uid,
                Token = refreshTokenString,
                UserAgent = userAgent,
                ExpiresAt = refreshTokenExpiration
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            user.LastAccessAt = SaoPauloDateTime.Now();
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = jwtToken,
                TokenExpiresAt = jwtExpiration,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiresAt = refreshTokenExpiration
            };
        }

        private async Task<LoginResponse> SignInGoogleUserAsync(GoogleTokenInfo profile, string userAgent, string? ipAddress, CancellationToken cancellationToken)
        {
            if (!profile.IsEmailVerified)
                throw new ApiException(HttpStatusCode.Unauthorized, "O e-mail Google precisa estar verificado.");

            var externalLogin = await _context.UserExternalLogins
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Provider == GoogleProvider && x.ProviderUserId == profile.Sub, cancellationToken);

            var user = externalLogin?.User;
            if (user is null)
            {
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

                    await _context.Users.AddAsync(user, cancellationToken);
                    await _context.UserSettings.AddAsync(new UserSettings { UserUid = user.Uid }, cancellationToken);
                }

                externalLogin = new UserExternalLogin
                {
                    UserUid = user.Uid,
                    Provider = GoogleProvider,
                    ProviderUserId = profile.Sub,
                    Email = profile.Email,
                    DisplayName = profile.Name
                };
                await _context.UserExternalLogins.AddAsync(externalLogin, cancellationToken);
            }
            else
            {
                var existingExternalLogin = externalLogin!;
                existingExternalLogin.Email = profile.Email;
                existingExternalLogin.DisplayName = profile.Name;
                existingExternalLogin.UpdatedAt = SaoPauloDateTime.Now();
            }

            if (user.IsDeleted)
                throw new ApiException(HttpStatusCode.Forbidden, "Sua conta foi removida e nao pode mais ser utilizada.");

            if (user.IsBlocked)
                throw new ApiException(HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");

            user.IsConfirmed = true;
            user.LastAccessAt = SaoPauloDateTime.Now();
            await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Success", IpAddress = ipAddress, UserAgent = userAgent }, cancellationToken);

            return await GenerateAuthResponseAsync(user, userAgent);
        }

        private async Task<ExternalAuthSession> GetOwnedGoogleSessionAsync(Guid sessionCode, string userAgent, string clientInstanceId, CancellationToken cancellationToken)
        {
            if (sessionCode == Guid.Empty)
                throw new ValidationException(nameof(sessionCode), "Informe o codigo da sessao Google.");

            var session = await _context.ExternalAuthSessions.FirstOrDefaultAsync(x => x.SessionCode == sessionCode && x.Provider == GoogleProvider, cancellationToken);
            if (session is null)
                throw new ApiException(HttpStatusCode.NotFound, "Sessao de login Google nao localizada.");

            if (!string.Equals(session.ClientUserAgent, userAgent, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(session.ClientInstanceId, clientInstanceId, StringComparison.Ordinal))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "Sessao Google nao pertence a este cliente.");
            }

            return session;
        }

        private static void ValidateGoogleSettings(global::eTasks_server.Models.Entities.Settings.ServerSettings settings)
        {
            if (!settings.GoogleOpenIdEnabled)
                throw new ApiException(HttpStatusCode.ServiceUnavailable, "Login Google esta desabilitado no servidor.");

            if (string.IsNullOrWhiteSpace(settings.GoogleOpenIdClientId))
                throw new ValidationException(nameof(settings.GoogleOpenIdClientId), "Configure o Client ID do Google.");

            if (string.IsNullOrWhiteSpace(settings.GoogleOpenIdClientSecret))
                throw new ValidationException(nameof(settings.GoogleOpenIdClientSecret), "Configure o Client Secret do Google.");

            if (string.IsNullOrWhiteSpace(settings.GoogleOpenIdStateCode))
                throw new ValidationException(nameof(settings.GoogleOpenIdStateCode), "Configure o codigo fixo de state do Google.");
        }

        private static string ResolveGoogleRedirectUri(string configuredRedirectUri, Uri? requestBaseUri)
        {
            if (!string.IsNullOrWhiteSpace(configuredRedirectUri))
                return configuredRedirectUri.Trim();

            if (requestBaseUri is null)
                throw new ValidationException(nameof(configuredRedirectUri), "Configure a Redirect URI do Google.");

            return new Uri(requestBaseUri, "api/v2/auth/google/callback").ToString();
        }

        private static string BuildGoogleAuthorizationUrl(string clientId, string redirectUri, string state)
        {
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

            return "https://accounts.google.com/o/oauth2/v2/auth?" + string.Join("&", query.Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
        }

        private async Task<GoogleTokenResponse> ExchangeGoogleCodeAsync(string code, string clientId, string clientSecret, string redirectUri, CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            }), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new ApiException(HttpStatusCode.Unauthorized, "Nao foi possivel trocar o codigo Google por tokens.");

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken);
            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.IdToken))
                throw new ApiException(HttpStatusCode.Unauthorized, "Resposta de token Google invalida.");

            return tokenResponse;
        }

        private async Task<GoogleTokenInfo> ValidateGoogleIdTokenAsync(string idToken, string clientId, CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var tokenInfo = await client.GetFromJsonAsync<GoogleTokenInfo>($"https://oauth2.googleapis.com/tokeninfo?id_token={WebUtility.UrlEncode(idToken)}", cancellationToken);
            if (tokenInfo is null)
                throw new ApiException(HttpStatusCode.Unauthorized, "ID token Google invalido.");

            if (!string.Equals(tokenInfo.Aud, clientId, StringComparison.Ordinal))
                throw new ApiException(HttpStatusCode.Unauthorized, "ID token Google emitido para outro Client ID.");

            if (!string.Equals(tokenInfo.Iss, "https://accounts.google.com", StringComparison.Ordinal)
                && !string.Equals(tokenInfo.Iss, "accounts.google.com", StringComparison.Ordinal))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "Emissor do ID token Google invalido.");
            }

            if (string.IsNullOrWhiteSpace(tokenInfo.Sub) || string.IsNullOrWhiteSpace(tokenInfo.Email))
                throw new ApiException(HttpStatusCode.Unauthorized, "Perfil Google incompleto.");

            return tokenInfo;
        }

        private string ProtectState(GoogleStatePayload payload)
        {
            var protectedJson = _secretProtector.Protect(JsonSerializer.Serialize(payload));
            return Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(protectedJson));
        }

        private GoogleStatePayload UnprotectState(string? state)
        {
            if (string.IsNullOrWhiteSpace(state))
                throw new ValidationException("state", "State obrigatorio.");

            var protectedJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(state));
            var json = _secretProtector.Unprotect(protectedJson);
            return JsonSerializer.Deserialize<GoogleStatePayload>(json) ?? throw new ValidationException("state", "State invalido.");
        }

        private GoogleAuthCallbackResult BuildGoogleCallbackResult(ExternalAuthSession session, GoogleStatePayload statePayload, bool success, string message)
        {
            var redirectUrl = BuildClientRedirectUrl(statePayload.ReturnUrl, session.SessionCode, success, session.ErrorCode);
            return new GoogleAuthCallbackResult
            {
                Success = success,
                SessionCode = session.SessionCode,
                UserAgent = session.ClientUserAgent,
                RedirectUrl = redirectUrl,
                Message = message
            };
        }

        private static string? BuildClientRedirectUrl(string? returnUrl, Guid sessionCode, bool success, string? errorCode)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
                return null;

            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
                return null;

            var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
            var url = $"{uri}{separator}googleSession={WebUtility.UrlEncode(sessionCode.ToString())}&success={success.ToString().ToLowerInvariant()}";
            if (!string.IsNullOrWhiteSpace(errorCode))
                url += $"&error={WebUtility.UrlEncode(errorCode)}";

            return url;
        }

        private static void MarkGoogleSessionFailed(ExternalAuthSession session, string errorCode, string errorDescription)
        {
            session.Status = ExternalAuthSessionStatus.Failed;
            session.ErrorCode = errorCode;
            session.ErrorDescription = errorDescription;
            session.CompletedAt = DateTime.UtcNow;
        }

        private static string NormalizeGoogleName(string? name, string email)
        {
            var value = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name.Trim();
            if (value.Length < 3)
                value = value.PadRight(3, '_');

            return value.Length > 30 ? value[..30] : value;
        }

        private const string GoogleProvider = "google";

        private sealed class GoogleStatePayload
        {
            public string FixedStateCode { get; set; } = string.Empty;
            public Guid SessionCode { get; set; }
            public string UserAgent { get; set; } = string.Empty;
            public string ClientInstanceId { get; set; } = string.Empty;
            public string? ReturnUrl { get; set; }
            public DateTime CreatedAtUtc { get; set; }
        }

        private sealed class GoogleTokenResponse
        {
            [JsonPropertyName("id_token")]
            public string IdToken { get; set; } = string.Empty;
        }

        private sealed class GoogleTokenInfo
        {
            [JsonPropertyName("iss")]
            public string Iss { get; set; } = string.Empty;

            [JsonPropertyName("aud")]
            public string Aud { get; set; } = string.Empty;

            [JsonPropertyName("sub")]
            public string Sub { get; set; } = string.Empty;

            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;

            [JsonPropertyName("email_verified")]
            public string EmailVerified { get; set; } = "false";

            [JsonIgnore]
            public bool IsEmailVerified => string.Equals(EmailVerified, "true", StringComparison.OrdinalIgnoreCase);

            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("picture")]
            public string? Picture { get; set; }
        }
    }
}
