using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IUserNotificationBLL
    {
        Task<PushDeviceRegistrationResponse> RegisterDeviceAsync(Guid userUid, RegisterPushDeviceRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<NotificationInboxItemResponse>> GetInboxAsync(Guid userUid, bool unreadOnly = false, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(Guid userUid, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(Guid userUid, Guid recipientId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(Guid userUid, CancellationToken cancellationToken = default);
    }
}
