using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Users.Admin.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Admin
{
    public static class UserAdminEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de administração de usuários, incluindo listagem, bloqueio, redefinição de senha, confirmação de conta, envio de e-mail de recuperação, obtenção de logs de login e exclusão permanente.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapUserAdminEndpoints()
            {
                var group = app.MapGroup("/users")
                    .WithTags("Gerenciamento de usuários")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription();

                group.ListarUsuarios()
                     .BloqueiarUsuario()
                     .RedefinirSenhaUsuario()
                     .ConfirmarManualmenteContaUsuario()
                     .EnviarCodigoResetSenhaPorEMail()
                     .ListarLogDeLoginsDoUsuario()
                     .RemoverPermanentementeUmUsuario()
                     .RemoveLoteDeUsuariosComnContasMarcadasParaExcusao();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Método que lista todos os usuários não-administradores do sistema, retornando suas informações básicas. Este endpoint é protegido por autorização e só pode ser acessado por administradores.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ListarUsuarios()
            {
                group.MapGet("/", async (IUserAdminBLL userAdminBLL) =>
                {
                    var users = await userAdminBLL.GetUsersAsync();
                    return Results.Ok(users);
                })
                .WithName("ListUsers")
                .WithSummary("Lista todos os usuários não-administradores.");

                return group;
            }

            /// <summary>
            /// Método que alterna o status de bloqueio de um usuário, permitindo que administradores bloqueiem ou desbloqueiem contas conforme necessário. O endpoint é protegido por autorização e só pode ser acessado por administradores.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder BloqueiarUsuario()
            {
                group.MapPatch("/{uid}/block", async (Guid uid, IUserAdminBLL userAdminBLL) =>
                {
                    await userAdminBLL.ToggleBlockAsync(uid);
                    return Results.Ok(new { Message = "Status de bloqueio alterado com sucesso." });
                })
                .WithName("ToggleUserBlock")
                .WithSummary("Bloqueia ou desbloqueia um usuário.");

                return group;
            }

            /// <summary>
            /// Método que permite a um administrador redefinir a senha de um usuário específico, fornecendo uma nova senha. O endpoint é protegido por autorização e só pode ser acessado por administradores.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder RedefinirSenhaUsuario()
            {
                group.MapPatch("/{uid}/password", async (Guid uid, [FromBody] AdminSetPasswordRequest request, IUserAdminBLL userAdminBLL) =>
                {
                    await userAdminBLL.SetPasswordAsync(uid, request.NewPassword);
                    return Results.Ok(new { Message = "Senha alterada com sucesso." });
                })
                .WithName("AdminSetUserPassword")
                .WithSummary("Redefine a senha de um usuário.");

                return group;
            }

            /// <summary>
            /// Método que permite a um administrador confirmar manualmente a conta de um usuário, caso haja algum problema com o processo de confirmação automática. O endpoint é protegido por autorização e só pode ser acessado por administradores.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ConfirmarManualmenteContaUsuario()
            {
                group.MapPatch("/{uid}/confirm", async (Guid uid, IUserAdminBLL userAdminBLL) =>
                {
                    await userAdminBLL.ConfirmAccountAsync(uid);
                    return Results.Ok(new { Message = "Conta confirmada com sucesso." });
                })
                .WithName("AdminConfirmUser")
                .WithSummary("Confirma manualmente a conta de um usuário.");

                return group;
            }

            /// <summary>
            /// Envia um e-mail de recuperação de senha para o usuário especificado, gerando um código de redefinição. Este endpoint é protegido por autorização e só pode ser acessado por administradores. O e-mail enviado contém instruções para o usuário redefinir sua senha usando o código fornecido.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder EnviarCodigoResetSenhaPorEMail()
            {
                group.MapPost("/{uid}/send-reset", async (Guid uid, IUserAdminBLL userAdminBLL) =>
                {
                    await userAdminBLL.SendPasswordResetEmailAsync(uid);
                    return Results.Ok(new { Message = "E-mail de recuperação enviado com sucesso." });
                })
                .WithName("AdminSendUserReset")
                .WithSummary("Gera um código e envia e-mail de recuperação de senha.");

                return group;
            }

            /// <summary>
            /// Lista o histórico de logins de um usuário específico, incluindo informações como data, hora e endereço IP de cada login. Este endpoint é protegido por autorização e só pode ser acessado por administradores, permitindo que eles monitorem a atividade de login dos usuários para fins de segurança e auditoria.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ListarLogDeLoginsDoUsuario()
            {
                group.MapGet("/{uid}/login-logs", async (Guid uid, IUserAdminBLL userAdminBLL) =>
                {
                    var logs = await userAdminBLL.GetLoginLogsAsync(uid);
                    return Results.Ok(logs);
                })
                .WithName("GetUserLoginLogs")
                .WithSummary("Obtém o histórico de logins de um usuário.");

                return group;
            }

            /// <summary>
            /// Remove permanentemente um usuário do sistema, excluindo todos os seus dados relacionados do banco de dados. Este endpoint é protegido por autorização e só pode ser acessado por administradores, garantindo que a exclusão de contas seja realizada de forma segura e controlada. Após a execução deste endpoint, o usuário não poderá mais acessar o sistema e todas as suas informações serão irrecuperáveis.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder RemoverPermanentementeUmUsuario()
            {
                group.MapDelete("/{uid}", async (Guid uid, IUserAdminBLL userAdminBLL) =>
                {
                    await userAdminBLL.DeletePermanentlyAsync(uid);
                    return Results.Ok(new { Message = "Conta removida permanentemente." });
                })
                .WithName("DeleteUserPermanently")
                .WithSummary("Remove permanentemente um usuário e todos os seus dados do banco.");

                return group;
            }

            /// <summary>
            /// Remove permanentemente todos os usuários que foram marcados para exclusão (soft-deleted) do sistema, liberando espaço no banco de dados e garantindo que contas inativas sejam completamente eliminadas. Este endpoint é protegido por autorização e só pode ser acessado por administradores, permitindo que eles gerenciem a limpeza de contas de forma eficiente. Após a execução deste endpoint, todas as contas com soft-delete ativo serão irrecuperáveis e não poderão mais acessar o sistema.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder RemoveLoteDeUsuariosComnContasMarcadasParaExcusao()
            {
                group.MapDelete("/purge-deleted", async (IUserAdminBLL userAdminBLL) =>
                {
                    var count = await userAdminBLL.PurgeDeletedUsersAsync();
                    return Results.Ok(new { Message = $"{count} conta(s) removida(s) permanentemente.", Count = count });
                })
                .WithName("PurgeDeletedUsers")
                .WithSummary("Remove permanentemente todos os usuários com soft-delete ativo.");

                return group;
            }
        }

    }
}
