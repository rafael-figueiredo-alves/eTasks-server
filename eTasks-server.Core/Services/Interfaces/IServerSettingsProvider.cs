using eTasks_server.Models.Entities.Settings;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IServerSettingsProvider
    {
        Task<ServerSettings> GetCurrentAsync(CancellationToken cancellationToken = default);
    }
}
