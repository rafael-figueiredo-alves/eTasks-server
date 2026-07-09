using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface das notificações de usuário
    /// </summary>
    public interface IUserNotificationBLL
    {
        /// <summary>
        /// Registra aparelho/dispositivo
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PushDeviceRegistrationResponse> RegisterDeviceAsync(Guid userUid, RegisterPushDeviceRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem caixa de notificações
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="unreadOnly"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyList<NotificationInboxItemResponse>> GetInboxAsync(Guid userUid, bool unreadOnly = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem total de notificações não lidas
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> GetUnreadCountAsync(Guid userUid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marca notificação como lida
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="recipientId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task MarkAsReadAsync(Guid userUid, Guid recipientId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marca todas as notificações como lidas
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task MarkAllAsReadAsync(Guid userUid, CancellationToken cancellationToken = default);
    }
}
