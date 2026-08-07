using eTasks_server.Core.Services.Models;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface para implementação de um logger de auditoria de operações.
    /// </summary>
    public interface IOperationAuditLogger
    {
        /// <summary>
        /// Registra um log de auditoria de operação de forma assíncrona.
        /// </summary>
        /// <param name="operationLog">Log de auditoria de operação a ser registrado</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns></returns>
        Task LogAsync(OperationAuditLog operationLog, CancellationToken cancellationToken = default);
    }
}
