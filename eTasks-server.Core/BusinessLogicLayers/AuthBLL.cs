using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Users;
using eTasks_server.Models.Utils;

namespace eTasks_server.Core.BusinessLogicLayers
{
    public class AuthBLL : IAuthBLL
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthBLL> _logger;

        public AuthBLL(AppDbContext context, IConfiguration configuration, IEmailService emailService, ILogger<AuthBLL> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Iniciando processo de registro para o e-mail: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Geral", "E-mail e senha são obrigatórios.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Tentativa de registro falhou. O e-mail {Email} já está em uso.", request.Email);
                throw new ValidationException("Email", "O e-mail informado já está cadastrado.");
            }

            // Tratamento da imagem Base64 (Opcional)
            string? photoPath = null;
            if (!string.IsNullOrWhiteSpace(request.PhotoBase64))
            {
                try
                {
                    var base64Data = request.PhotoBase64.Contains(',') ? request.PhotoBase64.Split(',')[1] : request.PhotoBase64;
                    var imageBytes = Convert.FromBase64String(base64Data);
                    string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                    
                    string fileName = $"{Guid.NewGuid()}.jpg";
                    photoPath = Path.Combine("uploads", "profiles", fileName); // Caminho relativo para recuperar via URL
                    await File.WriteAllBytesAsync(Path.Combine(directoryPath, fileName), imageBytes);
                }
                catch
                {
                    // Falha no processamento da imagem, continua sem foto.
                }
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                PhotoPath = photoPath,
                IsAdmin = false 
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Novo usuário criado no banco de dados. Uid: {Uid}, E-mail: {Email}", user.Uid, user.Email);

            // Generate Confirmation Token & Dispatch Email
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
            var confirmationLink = $"{baseUrl.TrimEnd('/')}/api/v2/auth/confirm-email?token={confirmationToken}";
            
            #pragma warning disable CS4014 // Fire and forget para não segurar a API
            _emailService.SendAccountConfirmationEmailAsync(user.Email, confirmationLink);
            #pragma warning restore CS4014

            // Auto login after register
            return await GenerateAuthResponseAsync(user, "Web"); // Mock do UserAgent
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
        {
            _logger.LogInformation("Iniciando tentativa de login para o usuário: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Geral", "Forneça o e-mail e a senha para continuar.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning("Falha de autenticação. E-mail não encontrado: {Email}", request.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = null, Status = "Failed", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Não encontramos uma conta com esse e-mail.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Falha de autenticação. Senha incorreta para o usuário: {Uid}", user.Uid);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Failed", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Senha incorreta. Verifique e tente novamente.");
            }

            if (user.IsBlocked)
            {
                _logger.LogWarning("Usuário {Uid} ({Email}) tentou logar, mas encontra-se bloqueado.", user.Uid, user.Email);
                await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Blocked", IpAddress = ipAddress, UserAgent = request.UserAgent });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            _logger.LogInformation("Usuário {Uid} autenticado com sucesso.", user.Uid);
            
            // Grava log de sucesso
            await _context.LoginLogs.AddAsync(new LoginLog { UserUid = user.Uid, Status = "Success", IpAddress = ipAddress, UserAgent = request.UserAgent });
            
            return await GenerateAuthResponseAsync(user, request.UserAgent);
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ValidationException("RefreshToken", "O token de renovação é obrigatório.");

            var tokenRecord = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken && !r.IsRevoked);

            if (tokenRecord == null)
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sessão inválida. Faça login novamente.");

            if (tokenRecord.ExpiresAt <= DateTime.UtcNow)
            {
                tokenRecord.IsRevoked = true;
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Sua sessão expirou. Faça login novamente.");
            }

            // Revogar o antigo e gerar novo (Refresh Token Rotation)
            tokenRecord.IsRevoked = true;
            
            return await GenerateAuthResponseAsync(tokenRecord.User!, request.UserAgent);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            _logger.LogInformation("Solicitação de recuperação de senha recebida para o e-mail: {Email}", request.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) 
            {
                _logger.LogInformation("O e-mail {Email} não foi localizado na base. Abortando silenciosamente por segurança.", request.Email);
                return true; // Para não revelar a existência do e-mail (segurança)
            }

            // Gera código de 6 digitos numéricos
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            var resetCode = new PasswordResetCode
            {
                UserUid = user.Uid,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15) // Código expira em 15 minutos
            };

            await _context.PasswordResetCodes.AddAsync(resetCode);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, code);
            
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) throw new ValidationException("Geral", "Solicitação inválida.");

            var resetCode = await _context.PasswordResetCodes
                .Where(c => c.UserUid == user.Uid && c.Code == request.Code && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (resetCode == null)
                throw new ValidationException("Code", "Código inválido ou já expirado.");

            resetCode.IsUsed = true;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Ao resetar a senha, invalida todos os Refresh Tokens abertos para deslogar todos devices
            var activeTokens = await _context.RefreshTokens.Where(r => r.UserUid == user.Uid && !r.IsRevoked).ToListAsync();
            foreach(var tk in activeTokens) tk.IsRevoked = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userUid, ChangePasswordRequest request)
        {
            _logger.LogInformation("Solicitação de mudança de senha para o usuário: {Uid}", userUid);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == userUid);
            if (user == null) throw new ValidationException("User", "Usuário não localizado.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("O usuário {Uid} tentou trocar a senha mas forneceu a senha atual incorretamente.", userUid);
                throw new ValidationException("CurrentPassword", "A senha atual está incorreta.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Invalida a sessão atual/outras pra forçar logoff
            var activeTokens = await _context.RefreshTokens.Where(r => r.UserUid == user.Uid && !r.IsRevoked).ToListAsync();
            foreach(var tk in activeTokens) tk.IsRevoked = true;

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

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == userUid);
                if (user == null || user.IsConfirmed) return true; // Se já confirmou, retorna true

                user.IsConfirmed = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha na validação do token de e-mail.");
                return false;
            }
        }

        #region Helper: Generate Tokens
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
            };

            if (user.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            else
                claims.Add(new Claim(ClaimTypes.Role, "User"));

            var jwtExpiration = DateTime.UtcNow.AddHours(4); // Válido por 4 horas (Access Token)

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

            // Generate Refresh Token
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(30); // Refresh por 30 dias

            var refreshToken = new RefreshToken
            {
                UserUid = user.Uid,
                Token = refreshTokenString,
                UserAgent = userAgent ?? "Unknown",
                ExpiresAt = refreshTokenExpiration 
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = jwtToken,
                TokenExpiresAt = jwtExpiration,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiresAt = refreshTokenExpiration
            };
        }
        #endregion
    }
}
