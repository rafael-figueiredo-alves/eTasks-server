using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Classe que implementa o registro de auditoria de operações sem realizar nenhuma ação.
    /// </summary>
    public class NoOpOperationAuditLogger : IOperationAuditLogger
    {
        /// <summary>
        /// Registra um log de auditoria de operação sem realizar nenhuma ação.
        /// </summary>
        /// <param name="operationLog"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task LogAsync(OperationAuditLog operationLog, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
