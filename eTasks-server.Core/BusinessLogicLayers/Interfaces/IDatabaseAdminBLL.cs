using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IDatabaseAdminBLL
    {
        Task<DatabaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
        Task<DatabaseBackupFileResponse> GenerateBackupAsync(CancellationToken cancellationToken = default);
        Task<DatabaseScriptExecutionResponse> ExecuteScriptAsync(DatabaseScriptExecutionRequest request, CancellationToken cancellationToken = default);
    }
}
