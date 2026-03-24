using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/auth").WithTags("Authentication");

            group.MapPost("/login", async (HttpContext context, [FromBody] LoginRequest request, IAuthBLL authBLL) =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                var response = await authBLL.LoginAsync(request, ip);
                return Results.Ok(response);
            })
            .WithName("UserLogin")
            .WithSummary("Realiza o login de um usuário retornando JWT e Refresh Token.")
            .Produces<LoginResponse>(StatusCodes.Status200OK);

            group.MapPost("/register", async ([FromBody] RegisterRequest request, IAuthBLL authBLL) =>
            {
                var response = await authBLL.RegisterAsync(request);
                return Results.Ok(response);
            })
            .WithName("UserRegister")
            .WithSummary("Registra um novo usuário no sistema.");

            group.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthBLL authBLL) =>
            {
                var response = await authBLL.RefreshTokenAsync(request);
                return Results.Ok(response);
            })
            .WithName("RefreshToken")
            .WithSummary("Troca um Refresh Token válido e não expirado por um novo Token JWT.");

            group.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, IAuthBLL authBLL) =>
            {
                var success = await authBLL.ForgotPasswordAsync(request);
                return Results.Ok(new { Success = success, Message = "Se o e-mail existir, um código foi enviado." });
            })
            .WithName("ForgotPassword")
            .WithSummary("Solicita envio de código de redefinição de senha.");

            group.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, IAuthBLL authBLL) =>
            {
                var success = await authBLL.ResetPasswordAsync(request);
                return Results.Ok(new { Success = success, Message = "Senha redefinida com êxito." });
            })
            .WithName("ResetPassword")
            .WithSummary("Valida o código de 6 dígitos e altera a senha da conta.");

            group.MapPost("/change-password", async (System.Security.Claims.ClaimsPrincipal user, [FromBody] ChangePasswordRequest request, IAuthBLL authBLL) =>
            {
                var userIdStr = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdStr, out Guid userUid)) return Results.Unauthorized();

                var success = await authBLL.ChangePasswordAsync(userUid, request);
                return Results.Ok(new { Success = success, Message = "Senha alterada com êxito." });
            })
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Altera a senha de um usuário autenticado (Requer envio do JWT no Authoization Bearer).");

            group.MapGet("/confirm-email", async ([FromQuery] string token, IAuthBLL authBLL) =>
            {
                var success = await authBLL.ConfirmEmailAsync(token);
                if (success)
                    return Results.Content("<h1>Conta confirmada com sucesso!</h1><p>Você pode fechar esta aba e retornar ao aplicativo.</p>", "text/html");
                else
                    return Results.Content("<h1>O link expirou ou é inválido.</h1><p>Você pode solicitar um novo código direto pelo eTasks.</p>", "text/html");
            })
            .WithName("ConfirmEmail")
            .WithSummary("Valida o token JWT e ativa a conta garantindo a veracidade do endereço de e-mail.");

            return app;
        }
    }
}
