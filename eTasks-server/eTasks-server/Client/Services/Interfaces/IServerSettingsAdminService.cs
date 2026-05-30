using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface da tabela de configurações do servidor
    /// </summary>
    public interface IServerSettingsAdminService
    {
        /// <summary>
        /// Obtem dados das configurações do servidor
        /// </summary>
        /// <param name="cancellationToken">Cancela operação</param>
        /// <returns>Configurações do servidor</returns>
        Task<ServerSettingsResponse> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza configurações do servidor
        /// </summary>
        /// <param name="request">Dados a atualizar</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Entidade das configurações atualizada</returns>
        Task<ServerSettingsResponse> UpdateAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Método para testar serviço de email configurado
        /// </summary>
        /// <param name="request">Dados da solicitação</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Resultado do teste de e-mail</returns>
        Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Método para testar conexão com o serviço OpenRouter que fornece interface única para agents de IA
        /// </summary>
        /// <param name="request">Parâmetros necessários para o teste</param>
        /// <param name="cancellationToken">cancelar operação</param>
        /// <returns>Resultado do teste</returns>
        Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Testa conectividade com base MongoDB para dados de auditoria
        /// </summary>
        /// <param name="request">Parâmetros necessários</param>
        /// <param name="cancellationToken">Cancela operação</param>
        /// <returns>Resultado do teste</returns>
        Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default);
    }
}
