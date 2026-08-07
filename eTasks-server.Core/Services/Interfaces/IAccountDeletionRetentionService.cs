namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface responsável por definir os métodos para a retenção e exclusão de contas expiradas.
    /// </summary>
    public interface IAccountDeletionRetentionService
    {
        /// <summary>
        /// Exclui contas expiradas do sistema.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Quantidade de contas excluídas</returns>
        Task<int> DeleteExpiredAccountsAsync(CancellationToken cancellationToken = default);
    }
}
