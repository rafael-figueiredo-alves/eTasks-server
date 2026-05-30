using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;
using Microsoft.AspNetCore.Components.Authorization;

namespace eTasks_server.Client.Services
{
    public class AdminNotificationService(IAdminNotificationBLL adminNotificationBLL,
                                          AuthenticationStateProvider authenticationStateProvider) : IAdminNotificationService
    {
        /// <summary>
        /// Envia notificação a usuário(s)
        /// </summary>
        /// <param name="request">Dados da Notificação</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Retorno do envio</returns>
        public async Task<SendAdminNotificationResponse> SendAsync(SendAdminNotificationRequest request, CancellationToken cancellationToken = default)
        {
            //Obtém status de autenticação
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();

            //Obtem a identificação do usuário
            var adminUserUid = state.User.GetRequiredUserUid();

            //Realiza o envio da notificação
            return await adminNotificationBLL.SendAsync(adminUserUid, request, cancellationToken);
        }
    }
}
