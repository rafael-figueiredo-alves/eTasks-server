namespace eTasks_server.Core.Services.Interfaces
{
    public interface IApplicationLogRetentionService
    {
        Task<int> ApplyRetentionAsync(CancellationToken cancellationToken = default);
    }
}
