using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interfaca de serviços administrativos da base de dados
    /// </summary>
    public interface IDatabaseAdminService
    {
        /// <summary>
        /// Obtem resumo so banco, dados de tamanho, quantidade de tabelas.
        /// </summary>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns></returns>
        Task<DatabaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Método para executar scripts na base de dados
        /// </summary>
        /// <param name="request">Corpo da solicitação com script a executar</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Resultado da operação</returns>
        Task<DatabaseScriptExecutionResponse> ExecuteScriptAsync(DatabaseScriptExecutionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Método para limpar a base de dados
        /// </summary>
        /// <param name="adminKey">Necessário informa a chave de Administrador</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Resultado da operação</returns>
        Task<DatabaseScriptExecutionResponse> ClearDatabaseAsync(string adminKey, CancellationToken cancellationToken = default);
    }
}
