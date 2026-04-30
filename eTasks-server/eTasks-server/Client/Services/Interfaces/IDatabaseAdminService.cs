using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IDatabaseAdminService
    {
        Task<DatabaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);
        Task<DatabaseScriptExecutionResponse> ExecuteScriptAsync(DatabaseScriptExecutionRequest request, CancellationToken cancellationToken = default);
        Task<DatabaseScriptExecutionResponse> ClearDatabaseAsync(string adminKey, CancellationToken cancellationToken = default);
    }
}
