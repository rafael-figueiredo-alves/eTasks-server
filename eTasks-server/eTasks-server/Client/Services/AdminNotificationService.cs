using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;
using Microsoft.AspNetCore.Components.Authorization;

namespace eTasks_server.Client.Services
{
    public class AdminNotificationService(
        IAdminNotificationBLL adminNotificationBLL,
        AuthenticationStateProvider authenticationStateProvider) : IAdminNotificationService
    {
        public async Task<SendAdminNotificationResponse> SendAsync(SendAdminNotificationRequest request, CancellationToken cancellationToken = default)
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            var adminUserUid = state.User.GetRequiredUserUid();
            return await adminNotificationBLL.SendAsync(adminUserUid, request, cancellationToken);
        }
    }
}
