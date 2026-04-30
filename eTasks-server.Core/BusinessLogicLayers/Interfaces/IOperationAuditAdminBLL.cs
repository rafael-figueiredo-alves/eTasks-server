using eTasks_server.Models.DTOs.OperationAudit.Requests;
using eTasks_server.Models.DTOs.OperationAudit.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IOperationAuditAdminBLL
    {
        Task<OperationAuditDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
        Task<OperationAuditLogPageResponse> GetEntriesAsync(OperationAuditLogQueryRequest request, CancellationToken cancellationToken = default);
        Task<OperationAuditBackupFileResponse> GenerateBackupAsync(CancellationToken cancellationToken = default);
        Task<long> ClearAsync(string adminKey, CancellationToken cancellationToken = default);
    }
}
