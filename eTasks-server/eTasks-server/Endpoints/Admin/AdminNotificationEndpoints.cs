using System.Security.Claims;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notifications.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Admin
{
    public static class AdminNotificationEndpoints
    {
        public static IEndpointRouteBuilder MapAdminNotificationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/admin/notifications")
                .WithTags("Notificacoes Administrativas")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription();

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

            return app;
        }
    }
}
