namespace eTasks_server.Core.Services.Interfaces
{
    public interface IAccountDeletionRetentionService
    {
        Task<int> DeleteExpiredAccountsAsync(CancellationToken cancellationToken = default);
    }
}
