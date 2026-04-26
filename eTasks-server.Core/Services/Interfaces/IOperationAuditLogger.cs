using eTasks_server.Core.Services.Models;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IOperationAuditLogger
    {
        Task LogAsync(OperationAuditLog operationLog, CancellationToken cancellationToken = default);
    }
}
