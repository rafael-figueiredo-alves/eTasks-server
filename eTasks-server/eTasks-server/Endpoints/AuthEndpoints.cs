using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace eTasks_server.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/auth").WithTags("Autenticação");

            LoginEndpoint(group);
            RegisterEndpoint(group);
            RefreshTokenEndpoint(group);
            LogoutEndpoint(group);
            ForgotPasswordEndpoint(group);
            ResetPasswordEndpoint(group);
            ChangePasswordEndpoint(group);
            ConfirmAccountEndpoint(group);

            return app;
        }

        #region Endpoints de autenticação
        private static void LoginEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/login", async (HttpContext context, [FromBody] LoginRequest request, IAuthBLL authBLL) =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                var response = await authBLL.LoginAsync(request, ip);
                SetRefreshTokenCookie(context, response, request.UserAgent);
                return Results.Ok(response);
            })
            .WithName("Login")
            .WithDisplayName("Login de Usuário")
            .WithSummary("Realiza o login de um usuário retornando JWT e Refresh Token.")
            .WithDescription("O JWT deve ser enviado no header Authorization Bearer para acessar rotas protegidas. O Refresh Token pode ser usado para obter um novo JWT sem precisar fazer login novamente, desde que o Refresh Token seja válido e não tenha expirado.")
            .Produces(StatusCodes.Status200OK, typeof(LoginResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse));
        }

        private static void RegisterEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/register", async (HttpContext context, [FromBody] RegisterRequest request, IAuthBLL authBLL) =>
            {
                var response = await authBLL.RegisterAsync(request);
                SetRefreshTokenCookie(context, response, request.UserAgent);
                return Results.Ok(response);
            })
            .WithName("UserRegister")
            .WithDisplayName("Registro de Usuário")
            .WithSummary("Registra um novo usuário no sistema.")
            .WithDescription("Após o registro, um e-mail de confirmação será enviado. O usuário deve clicar no link de confirmação para ativar a conta antes de poder fazer login.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK, typeof(LoginResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse));
        }

        private static void RefreshTokenEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/refresh", async (HttpContext context, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RefreshTokenRequest? request, IAuthBLL authBLL) =>
            {
                request ??= new RefreshTokenRequest();

                if (ShouldUseRefreshTokenCookie(request.UserAgent)
                    && string.IsNullOrWhiteSpace(request.RefreshToken)
                    && context.Request.Cookies.TryGetValue(Constants.RefreshTokenCookieName, out var cookieRefreshToken))
                {
                    request.RefreshToken = cookieRefreshToken;
                }

                var response = await authBLL.RefreshTokenAsync(request);
                SetRefreshTokenCookie(context, response, request.UserAgent);
                return Results.Ok(response);
            })
            .WithName("RefreshToken")
            .WithSummary("Troca um Refresh Token válido e não expirado por um novo Token JWT.")
            .WithDisplayName("Logar com Refresh Token")
            .WithDescription("Utilizado para obter um novo JWT sem precisar fazer login novamente, desde que o Refresh Token seja válido e não tenha expirado. O JWT retornado deve ser usado no header Authorization Bearer para acessar rotas protegidas.")
            .Produces(StatusCodes.Status200OK, typeof(LoginResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse));
        }

        private static void LogoutEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/logout", async (HttpContext context, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RefreshTokenRequest? request, IAuthBLL authBLL) =>
            {
                if (string.IsNullOrWhiteSpace(request?.UserAgent))
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["UserAgent"] = ["O UserAgent e obrigatorio."]
                    });
                }

                var refreshToken = request?.RefreshToken;
                if (ShouldUseRefreshTokenCookie(request?.UserAgent)
                    && string.IsNullOrWhiteSpace(refreshToken)
                    && context.Request.Cookies.TryGetValue(Constants.RefreshTokenCookieName, out var cookieRefreshToken))
                {
                    refreshToken = cookieRefreshToken;
                }

                await authBLL.RevokeRefreshTokenAsync(refreshToken);
                ClearRefreshTokenCookie(context);
                return Results.Ok(new { Message = "Logout realizado com sucesso." });
            })
            .WithName("Logout")
            .WithSummary("Revoga o refresh token atual e remove os cookies HttpOnly de autenticaÃ§Ã£o.")
            .WithDisplayName("Logout da API")
            .Produces(StatusCodes.Status200OK);
        }

        private static void ForgotPasswordEndpoint(RouteGroupBuilder group)
        {

            group.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, IAuthBLL authBLL) =>
            {
                var success = await authBLL.ForgotPasswordAsync(request);
                return Results.Ok(new PasswordResponse { Success = success, Message = "Se o e-mail existir, um código foi enviado." });
            })
            .WithName("ForgotPassword")
            .WithDisplayName("Esqueci minha senha")
            .WithSummary("Solicita envio de código de redefinição de senha.")
            .WithDescription("Utilizado para solicitar o envio de um código de redefinição de senha para o e-mail do usuário. Se o e-mail existir no sistema, um código de 6 dígitos será enviado para o endereço de e-mail fornecido. O código é necessário para validar a solicitação de redefinição de senha.")
            .Produces(StatusCodes.Status200OK, typeof(PasswordResponse));
        }

        private static void ResetPasswordEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, IAuthBLL authBLL) =>
            {
                var success = await authBLL.ResetPasswordAsync(request);
                return Results.Ok(new PasswordResponse { Success = success, Message = "Senha redefinida com êxito." });
            })
            .WithName("ResetPassword")
            .WithSummary("Valida o código de 6 dígitos e altera a senha da conta.")
            .WithDescription("Utilizado para redefinir a senha de um usuário. O código de 6 dígitos enviado para o e-mail do usuário deve ser fornecido junto com a nova senha. Se o código for válido e não tiver expirado, a senha da conta será alterada para a nova senha fornecida.")
            .WithDisplayName("Redefinir senha com código de 6 dígitos")
            .Produces(StatusCodes.Status200OK, typeof(PasswordResponse));
        }

        private static void ChangePasswordEndpoint(RouteGroupBuilder group)
        {
            group.MapPost("/change-password", async (System.Security.Claims.ClaimsPrincipal user, [FromBody] ChangePasswordRequest request, IAuthBLL authBLL) =>
            {
                var userIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdStr, out Guid userUid)) return Results.Unauthorized();

                var success = await authBLL.ChangePasswordAsync(userUid, request);
                return Results.Ok(new PasswordResponse { Success = success, Message = "Senha alterada com êxito." });
            })
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Altera a senha de um usuário autenticado (Requer envio do JWT no Authoization Bearer).")
            .WithDisplayName("Alterar senha de usuário autenticado")
            .WithDescription("Utilizado para alterar a senha de um usuário que já está autenticado. O usuário deve fornecer a senha atual e a nova senha desejada. O JWT do usuário deve ser enviado no header Authorization Bearer para acessar esta rota. Se a senha atual for válida, a senha da conta será alterada para a nova senha fornecida.")
            .Produces(StatusCodes.Status200OK, typeof(PasswordResponse));
        }

        private static void ConfirmAccountEndpoint(RouteGroupBuilder group)
        {
            group.MapGet("/confirm-email", async ([FromQuery] string token, IAuthBLL authBLL) =>
            {
                var success = await authBLL.ConfirmEmailAsync(token);
                if (success)
                    return Results.Content("<h1>Conta confirmada com sucesso!</h1><p>Você pode fechar esta aba e retornar ao aplicativo.</p>", "text/html");
                else
                    return Results.Content("<h1>O link expirou ou é inválido.</h1><p>Você pode solicitar um novo código direto pelo eTasks.</p>", "text/html");
            })
            .WithName("ConfirmEmail")
            .WithSummary("Valida o token JWT e ativa a conta garantindo a veracidade do endereço de e-mail.")
            .WithDescription("Utilizado para confirmar o endereço de e-mail de um usuário após o registro. O token JWT enviado no link de confirmação é validado e, se for válido, a conta do usuário é ativada. O usuário deve clicar no link de confirmação enviado para o e-mail após o registro para ativar a conta antes de poder fazer login.")
            .WithDisplayName("Confirmar e-mail de registro")
            .Produces(StatusCodes.Status200OK, contentType: "text/html");
        }

        private static void SetRefreshTokenCookie(HttpContext context, LoginResponse response, string? userAgent)
        {
            if (!ShouldUseRefreshTokenCookie(userAgent))
            {
                ClearRefreshTokenCookie(context);
                return;
            }

            context.Response.Cookies.Append(Constants.RefreshTokenCookieName, response.RefreshToken, BuildCookieOptions(response.RefreshTokenExpiresAt));
        }

        private static void ClearRefreshTokenCookie(HttpContext context)
        {
            var cookieOptions = BuildCookieOptions(null);
            context.Response.Cookies.Delete(Constants.RefreshTokenCookieName, cookieOptions);
        }

        private static CookieOptions BuildCookieOptions(DateTime? expiresAtUtc)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api",
                Expires = expiresAtUtc.HasValue ? new DateTimeOffset(expiresAtUtc.Value) : null
            };
        }

        private static bool ShouldUseRefreshTokenCookie(string? userAgent)
        {
            return string.Equals(userAgent, Constants.WebUserAgent, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
