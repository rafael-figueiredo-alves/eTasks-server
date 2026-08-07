namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço de retenção de logs da aplicação.
    /// </summary>
    public interface IApplicationLogRetentionService
    {
        /// <summary>
        /// Aplica a política de retenção de logs da aplicação, removendo logs antigos conforme definido na configuração.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>O número de logs removidos</returns>
        Task<int> ApplyRetentionAsync(CancellationToken cancellationToken = default);
    }
}
