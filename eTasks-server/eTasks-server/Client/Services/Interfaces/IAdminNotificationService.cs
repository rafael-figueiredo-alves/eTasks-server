using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Serviço de gerenciamento de Notificações
    /// </summary>
    public interface IAdminNotificationService
    {
        /// <summary>
        /// Use este método para enviar notificações a usuários
        /// </summary>
        /// <param name="request">Corpo da notificação a enviar</param>
        /// <param name="cancellationToken">Cancelar envio</param>
        /// <returns>Resposta da ação de envio</returns>
        Task<SendAdminNotificationResponse> SendAsync(SendAdminNotificationRequest request, CancellationToken cancellationToken = default);
    }
}
