using eTasks_server.Models.DTOs.OperationAudit.Requests;
using eTasks_server.Models.DTOs.OperationAudit.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface das operações com a auditoria de requisições registrada no serviço do MongoDB
    /// </summary>
    public interface IOperationAuditAdminService
    {
        /// <summary>
        /// Obter dados para o Dashboard
        /// </summary>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Dados para o dashboard</returns>
        Task<OperationAuditDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem entradas na base de dados de auditoria com solicitações feitas a API
        /// </summary>
        /// <param name="request">O que solicita verificar</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Retorno de registros sobre operações/requisições</returns>
        Task<OperationAuditLogPageResponse> GetEntriesAsync(OperationAuditLogQueryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Método que apaga base de auditoria
        /// </summary>
        /// <param name="adminKey">Necessário informar chave de Administrador</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Retorno da operação</returns>
        Task<long> ClearAsync(string adminKey, CancellationToken cancellationToken = default);
    }
}
