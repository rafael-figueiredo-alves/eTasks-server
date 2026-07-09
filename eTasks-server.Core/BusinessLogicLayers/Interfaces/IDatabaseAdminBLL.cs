using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de operações com banco de dados
    /// </summary>
    public interface IDatabaseAdminBLL
    {
        /// <summary>
        /// Obtem resumo do banco de dados
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DatabaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gera backup
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DatabaseBackupFileResponse> GenerateBackupAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executa script
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DatabaseScriptExecutionResponse> ExecuteScriptAsync(DatabaseScriptExecutionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Limpa base de dados
        /// </summary>
        /// <param name="adminKey">chave administrativa</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DatabaseScriptExecutionResponse> ClearDatabaseAsync(string adminKey, CancellationToken cancellationToken = default);
    }
}
