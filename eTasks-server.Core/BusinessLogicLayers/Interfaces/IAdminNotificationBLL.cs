using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IAdminNotificationBLL
    {
        Task<SendAdminNotificationResponse> SendAsync(Guid adminUserUid, SendAdminNotificationRequest request, CancellationToken cancellationToken = default);
    }
}
