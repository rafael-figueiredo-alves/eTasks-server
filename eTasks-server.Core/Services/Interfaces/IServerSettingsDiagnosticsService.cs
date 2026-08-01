using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço de diagnóstico de configurações do servidor.
    /// </summary>
    public interface IServerSettingsDiagnosticsService
    {
        /// <summary>
        /// Realiza o teste de envio de e-mail com base nas configurações fornecidas.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Testa conexão com o OpenRouter usando as configurações fornecidas.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Testa a conexão com o MongoDB usando as configurações fornecidas.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
    }
}
