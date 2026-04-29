using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;

namespace eTasks_server.Client.Services
{
    public class DatabaseAdminService(IDatabaseAdminBLL bll) : IDatabaseAdminService
    {
        public Task<DatabaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
            => bll.GetOverviewAsync(cancellationToken);

        public Task<DatabaseScriptExecutionResponse> ExecuteScriptAsync(DatabaseScriptExecutionRequest request, CancellationToken cancellationToken = default)
            => bll.ExecuteScriptAsync(request, cancellationToken);
    }
}
