using eTasks_server.Models.Version;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IVersionService
    {
        Task<eTasksVersion> GetVersionAsync();
        Task<bool> SaveVersionAsync(eTasksVersion version);
    }
}
