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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Core.BusinessLogicLayers.Auth
{
    public class AuthBLL : BaseBLL<IAuthBLL>, IAuthBLL
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISecretProtector _secretProtector;

        public AuthBLL(AppDbContext context, IConfiguration configuration, IEmailService emailService, ISecretProtector secretProtector, ILogger<IAuthBLL> logger) : base(context, logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _secretProtector = secretProtector;
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
    }
}
