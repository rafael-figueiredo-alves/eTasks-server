using eTasks_server.Models.DTOs.OperationAudit.Requests;
using eTasks_server.Models.DTOs.OperationAudit.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de operações de auditoria administrativa
    /// </summary>
    public interface IOperationAuditAdminBLL
    {
        /// <summary>
        /// Obter dados para dashboard
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationAuditDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obter entradas de auditoria
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationAuditLogPageResponse> GetEntriesAsync(OperationAuditLogQueryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gerar backup do log do MongoDB
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<OperationAuditBackupFileResponse> GenerateBackupAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Limpar dados do mongodb
        /// </summary>
        /// <param name="adminKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<long> ClearAsync(string adminKey, CancellationToken cancellationToken = default);
    }
}
