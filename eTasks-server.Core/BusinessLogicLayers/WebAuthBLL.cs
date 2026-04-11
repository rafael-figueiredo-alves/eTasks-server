using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace eTasks_server.Core.BusinessLogicLayers
{
    public class WebAuthBLL : IWebAuthBLL
    {
        private readonly AppDbContext _context;
        private readonly ILogger<IWebAuthBLL> _logger;

        public WebAuthBLL(AppDbContext context, ILogger<IWebAuthBLL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LoginAsync(HttpContext httpContext, WebLoginRequest request, string? ipAddress)
        {
            _logger.LogInformation("Iniciando login web administrativo para o e-mail: {Email}", request.Email);

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException("Geral", "Forneça o e-mail e a senha para continuar.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

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
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Conta removida. Nao e possivel acessar o sistema.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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

            if (!user.IsAdmin)
            {
                throw new ApiException(System.Net.HttpStatusCode.Forbidden, "Acesso restrito. Apenas administradores podem acessar o sistema.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Uid.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(Constants.UserAgentClaimType, Constants.WebAdminUserAgent)
            };

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

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

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

        public async Task LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
