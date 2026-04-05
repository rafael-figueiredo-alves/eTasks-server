using eTasks_server.Models.Entities.Version;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IVersionService
    {
        Task<eTasksVersion> GetVersionAsync();
        Task<bool> SaveVersionAsync(eTasksVersion version);
    }
}
