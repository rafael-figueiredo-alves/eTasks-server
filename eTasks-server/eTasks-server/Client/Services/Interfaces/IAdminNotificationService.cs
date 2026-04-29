using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IAdminNotificationService
    {
        Task<SendAdminNotificationResponse> SendAsync(SendAdminNotificationRequest request, CancellationToken cancellationToken = default);
    }
}
