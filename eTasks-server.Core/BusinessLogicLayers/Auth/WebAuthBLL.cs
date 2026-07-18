using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace eTasks_server.Core.BusinessLogicLayers.Auth
{
    public class WebAuthBLL : BaseBLL<IWebAuthBLL>, IWebAuthBLL
    {
        private readonly IConfiguration _configuration;
        private readonly ISecretProtector _secretProtector;

        public WebAuthBLL(AppDbContext context, IConfiguration configuration, ISecretProtector secretProtector, ILogger<IWebAuthBLL> logger) : base(context, logger)
        {
            _configuration = configuration;
            _secretProtector = secretProtector;
        }

        /// <summary>
        /// Realiza Login no painel Administrativo do servidor com dados informados
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="request">Dados passados para login</param>
        /// <param name="ipAddress">Endereço IP</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="ApiException"></exception>
        public async Task LoginAsync(HttpContext httpContext, WebLoginRequest request, string? ipAddress)
        {
            _logger.LogInformation("Iniciando login web administrativo para o e-mail: {Email}", request.Email);

            // Valida se e-mail e/ou senha foram informados vazios ou em branco
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("Geral", "Forneça o e-mail e a senha para continuar.");
            }

            // Tenta obter usuário (conta)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Se usuário não existir
            if (user is null)
            {
                await _context.LoginLogs.AddAsync(new LoginLog
                {
                    UserUid = null,
                    Status = "Failed",
                    IpAddress = ipAddress,
                    UserAgent = Constants.WebAdminUserAgent
                });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Não encontramos uma conta com esse e-mail.");
            }

            // Se conta encontrada se encontrar removida
            if (user.IsDeleted)
            {
                await _context.LoginLogs.AddAsync(new LoginLog
                {
                    UserUid = user.Uid,
                    Status = "Blocked",
                    IpAddress = ipAddress,
                    UserAgent = Constants.WebAdminUserAgent
                });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Conta removida. Não é possível acessar o sistema.");
            }

            // Valida se senha informada é válida
            if (!BCrypt.Net.BCrypt.Verify(request.Password, _secretProtector.Unprotect(user.PasswordHash)))
            {
                await _context.LoginLogs.AddAsync(new LoginLog
                {
                    UserUid = user.Uid,
                    Status = "Failed",
                    IpAddress = ipAddress,
                    UserAgent = Constants.WebAdminUserAgent
                });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Senha incorreta. Verifique e tente novamente.");
            }

            // Valida se usuário está bloqeuado
            if (user.IsBlocked)
            {
                await _context.LoginLogs.AddAsync(new LoginLog
                {
                    UserUid = user.Uid,
                    Status = "Blocked",
                    IpAddress = ipAddress,
                    UserAgent = Constants.WebAdminUserAgent
                });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            // Valida se usuário encontrado não é um administrador
            if (!user.IsAdmin)
            {
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Acesso restrito. Apenas administradores podem acessar o sistema.");
            }

            // Monta lista de Claims do Token de acesso
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Admin"), // Adiciona que o papel do usuário é administrador
                new Claim(Constants.UserAgentClaimType, Constants.WebAdminUserAgent)
            };

            // Obtem foto de perfil do usuário
            if (!string.IsNullOrWhiteSpace(user.PhotoPath))
            {
                claims.Add(new Claim(Constants.PhotoPathClaimType, user.PhotoPath));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                AllowRefresh = true,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(14)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            // Realiza login, gravando Cookie de autenticação no browser
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            // Registra informações de login no log
            await _context.LoginLogs.AddAsync(new LoginLog
            {
                UserUid = user.Uid,
                Status = "Success",
                IpAddress = ipAddress,
                UserAgent = Constants.WebAdminUserAgent
            });
            user.LastAccessAt = SaoPauloDateTime.Now();
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Método para registrar conta de Administrador
        /// </summary>
        /// <param name="request">Dados da requisição</param>
        /// <param name="ipAddress">Endereço IP do solicitante</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="ApiException"></exception>
        public async Task RegisterAdminAsync(WebAdminRegisterRequest request, string? ipAddress)
        {
            _logger.LogInformation("Iniciando cadastro web administrativo para o e-mail: {Email}", request.Email);

            // Valida se algum dado obrigatório foi enviado vazio ou em branco
            if (string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password)
                || string.IsNullOrWhiteSpace(request.DisplayName)
                || string.IsNullOrWhiteSpace(request.AdminKey))
            {
                throw new ValidationException("Geral", "Usuário, senha, nome de exibição e chave administrativa são obrigatórios.");
            }

            // Obtém chave de Administrador das configurações da aplicação
            var configuredAdminKey = _configuration[Constants.AdminApiKeyConfig];

            // Se chave Administrativa não puder ser obtida
            if (string.IsNullOrWhiteSpace(configuredAdminKey))
            {
                throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "A chave administrativa não foi configurada no servidor.");
            }

            // Retorna se chave administrativa for inválida
            if (!string.Equals(request.AdminKey.Trim(), configuredAdminKey.Trim(), StringComparison.Ordinal))
            {
                await _context.LoginLogs.AddAsync(new LoginLog
                {
                    UserUid = null,
                    Status = "Failed",
                    IpAddress = ipAddress,
                    UserAgent = Constants.WebAdminUserAgent
                });
                await _context.SaveChangesAsync();
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Chave administrativa inválida.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // Valida se e-mail informado já se encontra em uso por outro usuário
            var emailAlreadyInUse = await _context.Users.AnyAsync(x => x.Email == normalizedEmail);
            if (emailAlreadyInUse)
            {
                throw new ValidationException("Email", "Já existe um usuário cadastrado com esse identificador.");
            }

            // Grava dados do novo usuário
            var user = new User
            {
                Name = request.DisplayName.Trim(),
                Email = normalizedEmail,
                PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(request.Password)),
                IsAdmin = true,
                IsConfirmed = true
            };

            // Adiciona o usuário ao Banco
            await _context.Users.AddAsync(user);

            // Adiciona as configurações de usuário do novo usuário
            await _context.UserSettings.AddAsync(new UserSettings { UserUid = user.Uid });

            // Adiciona dados ao log de logins
            await _context.LoginLogs.AddAsync(new LoginLog
            {
                UserUid = user.Uid,
                Status = "Success",
                IpAddress = ipAddress,
                UserAgent = Constants.WebAdminUserAgent
            });

            // Salva dados
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Método para deslogar da conta de Administrador
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task LogoutAsync(HttpContext httpContext)
        {
            // Realiza o logout
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
