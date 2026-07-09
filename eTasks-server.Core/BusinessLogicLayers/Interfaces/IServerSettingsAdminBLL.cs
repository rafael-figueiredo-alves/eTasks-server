using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface das configurações de servidor
    /// </summary>
    public interface IServerSettingsAdminBLL
    {
        /// <summary>
        /// Obtem configurações
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsResponse> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza configurações
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsResponse> UpdateAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Testa envio de e-mails
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Testa acesso a OpenRouter (IA)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Teste serviço/conexão com MongoDB
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
    }
}
