using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de notificações de administrador
    /// </summary>
    public interface IAdminNotificationBLL
    {
        /// <summary>
        /// Método para enviar notificações
        /// </summary>
        /// <param name="adminUserUid">UID do administrador</param>
        /// <param name="request">Corpo com dados da notificação</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta do retorno do envio</returns>
        Task<SendAdminNotificationResponse> SendAsync(Guid adminUserUid, SendAdminNotificationRequest request, CancellationToken cancellationToken = default);
    }
}
