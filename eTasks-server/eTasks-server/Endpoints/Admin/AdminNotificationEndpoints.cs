using System.Security.Claims;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notifications.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Admin
{
    public static class AdminNotificationEndpoints
    {
        extension (IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados às notificações administrativas. Esses endpoints permitem que os usuários administradores enviem notificações para outros usuários do sistema. O grupo de endpoints é protegido por autorização, exigindo que o usuário tenha a permissão "WebAdmin" para acessá-los. Além disso, esses endpoints são excluídos da descrição da API, o que significa que eles não aparecerão na documentação gerada automaticamente, como Swagger.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapAdminNotificationEndpoints()
            {
                var group = app.MapGroup("/admin/notifications")
                    .WithTags("Notificacoes Administrativas")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription();

                group.SendNotification();

                return app;
            }
        }

        extension (RouteGroupBuilder group)
        {
            /// <summary>
            /// Método responsável por enviar uma notificação administrativa para um ou mais usuários. O remetente da notificação é o usuário administrador autenticado que faz a requisição. O corpo da requisição deve conter as informações necessárias para criar a notificação, como o tipo de destino, os UIDs dos usuários destinatários, o título, o corpo da mensagem, a URL de ação (opcional) e os dados adicionais em formato JSON (opcional).
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder SendNotification()
            {
                group.MapPost("/send", async (
                                              ClaimsPrincipal user,
                                              [FromBody] SendAdminNotificationRequest request,
                                              IAdminNotificationBLL adminNotificationBLL,
                                              CancellationToken cancellationToken) =>
                                        {
                                            var adminUserUid = user.GetRequiredUserUid();
                                            return Results.Ok(await adminNotificationBLL.SendAsync(adminUserUid, request, cancellationToken));
                                        })
                .WithName("AdminSendNotification");

                return group;
            }
        }
    }
}
