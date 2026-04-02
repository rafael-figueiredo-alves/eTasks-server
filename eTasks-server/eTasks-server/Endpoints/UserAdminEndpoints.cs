using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints
{
    public static class UserAdminEndpoints
    {
        public static IEndpointRouteBuilder MapUserAdminEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/users")
                .WithTags("Gerenciamento de usuários")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription();

            group.MapGet("/", async (IUserAdminBLL userAdminBLL) =>
            {
                var users = await userAdminBLL.GetUsersAsync();
                return Results.Ok(users);
            })
            .WithName("ListUsers")
            .WithSummary("Lista todos os usuários não-administradores.");

            group.MapPatch("/{uid}/block", async (Guid uid, IUserAdminBLL userAdminBLL) =>
            {
                await userAdminBLL.ToggleBlockAsync(uid);
                return Results.Ok(new { Message = "Status de bloqueio alterado com sucesso." });
            })
            .WithName("ToggleUserBlock")
            .WithSummary("Bloqueia ou desbloqueia um usuário.");

            group.MapPatch("/{uid}/password", async (Guid uid, [FromBody] AdminSetPasswordRequest request, IUserAdminBLL userAdminBLL) =>
            {
                await userAdminBLL.SetPasswordAsync(uid, request.NewPassword);
                return Results.Ok(new { Message = "Senha alterada com sucesso." });
            })
            .WithName("AdminSetUserPassword")
            .WithSummary("Redefine a senha de um usuário.");

            group.MapPatch("/{uid}/confirm", async (Guid uid, IUserAdminBLL userAdminBLL) =>
            {
                await userAdminBLL.ConfirmAccountAsync(uid);
                return Results.Ok(new { Message = "Conta confirmada com sucesso." });
            })
            .WithName("AdminConfirmUser")
            .WithSummary("Confirma manualmente a conta de um usuário.");

            group.MapPost("/{uid}/send-reset", async (Guid uid, IUserAdminBLL userAdminBLL) =>
            {
                await userAdminBLL.SendPasswordResetEmailAsync(uid);
                return Results.Ok(new { Message = "E-mail de recuperação enviado com sucesso." });
            })
            .WithName("AdminSendUserReset")
            .WithSummary("Gera um código e envia e-mail de recuperação de senha.");

            group.MapGet("/{uid}/login-logs", async (Guid uid, IUserAdminBLL userAdminBLL) =>
            {
                var logs = await userAdminBLL.GetLoginLogsAsync(uid);
                return Results.Ok(logs);
            })
            .WithName("GetUserLoginLogs")
            .WithSummary("Obtém o histórico de logins de um usuário.");

            return app;
        }
    }
}
