using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;

namespace eTasks_server.Core.Services
{
    public class NoOpOperationAuditLogger : IOperationAuditLogger
    {
        public Task LogAsync(OperationAuditLog operationLog, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
