using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.OperationAudit.Requests;
using eTasks_server.Models.DTOs.OperationAudit.Responses;

namespace eTasks_server.Client.Services
{
    public class OperationAuditAdminService(IOperationAuditAdminBLL bll) : IOperationAuditAdminService
    {
        public Task<OperationAuditDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
            => bll.GetDashboardAsync(cancellationToken);

        public Task<OperationAuditLogPageResponse> GetEntriesAsync(OperationAuditLogQueryRequest request, CancellationToken cancellationToken = default)
            => bll.GetEntriesAsync(request, cancellationToken);

        public Task<long> ClearAsync(string adminKey, CancellationToken cancellationToken = default)
            => bll.ClearAsync(adminKey, cancellationToken);
    }
}
