using System.Security.Claims;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notifications.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Notifications
{
    public static class NotificationsEndpoints
    {
        public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/notifications")
                .WithTags("Notificacoes")
                .RequireAuthorization();

            group.MapPost("/devices", async (
                ClaimsPrincipal user,
                [FromBody] RegisterPushDeviceRequest request,
                IUserNotificationBLL notificationBLL,
                CancellationToken cancellationToken) =>
            {
                var userUid = user.GetRequiredUserUid();
                return Results.Ok(await notificationBLL.RegisterDeviceAsync(userUid, request, cancellationToken));
            })
            .WithName("RegisterPushDevice")
            .WithSummary("Registra ou atualiza o dispositivo do usuario para receber notificacoes.");

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
            .WithSummary("Lista notificacoes do usuario autenticado.");

            group.MapGet("/unread-count", async (
                ClaimsPrincipal user,
                IUserNotificationBLL notificationBLL,
                CancellationToken cancellationToken) =>
            {
                var userUid = user.GetRequiredUserUid();
                return Results.Ok(new { Count = await notificationBLL.GetUnreadCountAsync(userUid, cancellationToken) });
            })
            .WithName("GetUnreadNotificationCount")
            .WithSummary("Retorna a quantidade de notificacoes nao lidas.");

            group.MapPatch("/{recipientId:guid}/read", async (
                ClaimsPrincipal user,
                Guid recipientId,
                IUserNotificationBLL notificationBLL,
                CancellationToken cancellationToken) =>
            {
                var userUid = user.GetRequiredUserUid();
                await notificationBLL.MarkAsReadAsync(userUid, recipientId, cancellationToken);
                return Results.Ok(new { Message = "Notificacao marcada como lida." });
            })
            .WithName("MarkNotificationAsRead")
            .WithSummary("Marca uma notificacao como lida.");

            group.MapPatch("/read-all", async (
                ClaimsPrincipal user,
                IUserNotificationBLL notificationBLL,
                CancellationToken cancellationToken) =>
            {
                var userUid = user.GetRequiredUserUid();
                await notificationBLL.MarkAllAsReadAsync(userUid, cancellationToken);
                return Results.Ok(new { Message = "Notificacoes marcadas como lidas." });
            })
            .WithName("MarkAllNotificationsAsRead")
            .WithSummary("Marca todas as notificacoes do usuario como lidas.");

            return app;
        }
    }
}
