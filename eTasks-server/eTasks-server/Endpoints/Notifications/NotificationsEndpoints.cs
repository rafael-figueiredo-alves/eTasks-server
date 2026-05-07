using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eTasks_server.Endpoints.Notifications
{
    /// <summary>
    /// Classe de endpoints do recurso de notificações, responsável por mapear as rotas relacionadas às notificações do usuário. Esta classe define os endpoints para registrar ou atualizar o dispositivo do usuário para receber notificações push, listar as notificações do usuário autenticado, obter a quantidade de notificações não lidas, marcar uma notificação como lida e marcar todas as notificações como lidas. Todos os endpoints exigem autenticação e estão agrupados sob a rota "/notifications". A documentação de cada endpoint inclui um resumo, descrição detalhada e os códigos de status HTTP que podem ser retornados.
    /// </summary>
    public static class NotificationsEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            public IEndpointRouteBuilder MapNotificationsEndpoints()
            {
                var group = app.MapGroup("/notifications")
                    .WithTags("Notificacoes")
                    .RequireAuthorization();

                group
                     .RegisterPushDevice()
                     .ListUserPushNotifications()
                     .GetUnreadNotificationCount()
                     .MarkAsRead()
                     .MarkAllAsRead();

                return app;
            }
        }

        /// <summary>
        /// Extensão com métodos para configurar os endpoints relacionados às notificações do usuário. Esta extensão é aplicada a um RouteGroupBuilder, permitindo que os endpoints sejam agrupados sob uma rota comum ("/notifications") e compartilhem configurações como autenticação e tags. Cada método nesta extensão define um endpoint específico para lidar com as operações de notificações, como registrar dispositivos, listar notificações, obter contagem de notificações não lidas e marcar notificações como lidas. A estrutura de extensão facilita a organização e manutenção dos endpoints relacionados às notificações em um único local.
        /// </summary>
        /// <param name="group"></param>
        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Método para registrar ou atualizar o dispositivo do usuário para receber notificações push. O usuário deve fornecer um token de dispositivo válido e o tipo de plataforma (iOS ou Android). Se o dispositivo já estiver registrado, as informações serão atualizadas.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder RegisterPushDevice()
            {
                group.MapPost("/devices", async (ClaimsPrincipal user,
                                                [FromBody] RegisterPushDeviceRequest request,
                                                IUserNotificationBLL notificationBLL,
                                                CancellationToken cancellationToken) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    return Results.Ok(await notificationBLL.RegisterDeviceAsync(userUid, request, cancellationToken));
                })
                .WithName("RegisterPushDevice")
                .WithSummary("Registra ou atualiza o dispositivo do usuário para receber notificações.")
                .WithDescription("Este endpoint permite que o usuário registre ou atualize as informações do seu dispositivo para receber notificações push. O usuário deve fornecer um token de dispositivo válido e o tipo de plataforma (iOS ou Android). Se o dispositivo já estiver registrado, as informações serão atualizadas.")
                .Produces(StatusCodes.Status200OK, typeof(PushDeviceRegistrationResponse))
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Método para listar as notificações do usuário autenticado, com a opção de filtrar apenas as notificações não lidas. As notificações são retornadas em ordem decrescente de data de criação.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder ListUserPushNotifications()
            {
                group.MapGet("/", async (
                                         ClaimsPrincipal user,
                                         [FromQuery] bool unreadOnly,
                                         IUserNotificationBLL notificationBLL,
                                         CancellationToken cancellationToken) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    return Results.Ok(await notificationBLL.GetInboxAsync(userUid, unreadOnly, cancellationToken));
                })
                .WithName("ListNotifications")
                .WithSummary("Lista notificações do usuário autenticado.")
                .WithDescription("Este endpoint retorna uma lista de notificações para o usuário autenticado. O usuário pode optar por receber apenas as notificações não lidas, definindo o parâmetro 'unreadOnly' como true. As notificações são retornadas em ordem decrescente de data de criação.")
                .Produces(StatusCodes.Status200OK, typeof(IReadOnlyList<NotificationInboxItemResponse>))
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Retorna a quantidade de notificações não lidas para o usuário autenticado. Este endpoint é útil para exibir um contador de notificações não lidas na interface do usuário, permitindo que o usuário saiba quantas notificações ainda precisam ser visualizadas.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder GetUnreadNotificationCount()
            {
                group.MapGet("/unread-count", async (
                ClaimsPrincipal user,
                IUserNotificationBLL notificationBLL,
                CancellationToken cancellationToken) =>
                {
                    var userUid = user.GetRequiredUserUid();
                    return Results.Ok(new { Count = await notificationBLL.GetUnreadCountAsync(userUid, cancellationToken)
                    });
                })
                .WithName("GetUnreadNotificationCount")
                .WithSummary("Retorna a quantidade de notificações não lidas.")
                .WithDescription("Este endpoint retorna a quantidade de notificações não lidas para o usuário autenticado. Este endpoint é útil para exibir um contador de notificações não lidas na interface do usuário, permitindo que o usuário saiba quantas notificações ainda precisam ser visualizadas.")
                .Produces(StatusCodes.Status200OK, typeof(int))
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Mapeia um endpoint PATCH que permite ao usuário marcar uma notificação como lida para um destinatário
            /// específico.
            /// </summary>
            /// <remarks>O endpoint exige autenticação e um identificador de destinatário válido.
            /// Retorna status HTTP 200 em caso de sucesso, 400 para solicitações inválidas, 401 se não autenticado, 404
            /// se a notificação não for encontrada e 500 para erros internos.</remarks>
            /// <returns>O próprio <see cref="RouteGroupBuilder"/>, permitindo o encadeamento de chamadas de configuração de
            /// rotas.</returns>
            public RouteGroupBuilder MarkAsRead()
            {
                group.MapPatch("/{recipientId:guid}/read", async (
                                                                 ClaimsPrincipal user,
                                                                 Guid recipientId,
                                                                 IUserNotificationBLL notificationBLL,
                                                                 CancellationToken cancellationToken) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    await notificationBLL.MarkAsReadAsync(userUid, recipientId, cancellationToken);

                    return Results.Ok(new { Message = "Notificação marcada como lida." });
                })
                .WithName("MarkNotificationAsRead")
                .WithSummary("Marca uma notificação como lida.")
                .WithDescription("Este endpoint permite que o usuário marque uma notificação específica como lida, usando o ID do destinatário da notificação. O usuário deve fornecer um ID de destinatário válido para que a notificação seja marcada como lida.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Adiciona um endpoint à rota do grupo que permite ao usuário marcar todas as suas notificações como
            /// lidas.
            /// </summary>
            /// <remarks>O endpoint exige que o usuário esteja autenticado. Ao ser chamado, todas as
            /// notificações associadas ao usuário autenticado serão marcadas como lidas. O endpoint retorna status HTTP
            /// 200 em caso de sucesso, 400 para solicitações inválidas, 401 se o usuário não estiver autenticado, 404
            /// se o usuário não for encontrado e 500 para erros internos.</remarks>
            /// <returns>O próprio <see cref="RouteGroupBuilder"/>, permitindo o encadeamento de chamadas de configuração
            /// adicionais.</returns>
            public RouteGroupBuilder MarkAllAsRead()
            {
                group.MapPatch("/read-all", async (
                                                   ClaimsPrincipal user,
                                                   IUserNotificationBLL notificationBLL,
                                                   CancellationToken cancellationToken) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    await notificationBLL.MarkAllAsReadAsync(userUid, cancellationToken);

                    return Results.Ok(new { Message = "Notificações marcadas como lidas." });
                })
                .WithName("MarkAllNotificationsAsRead")
                .WithSummary("Marca todas as notificações do usuário como lidas.")
                .WithDescription("Este endpoint permite que o usuário marque todas as suas notificações como lidas de uma só vez. O usuário deve estar autenticado para usar este endpoint, e todas as notificações associadas ao usuário serão marcadas como lidas.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }        
        }
    }
}
