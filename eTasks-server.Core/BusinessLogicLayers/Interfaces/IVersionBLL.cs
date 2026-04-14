using eTasks_server.Models.Entities.Version;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IVersionBLL
    {
        Task<eTasksVersion> GetVersionAsync();
        Task<bool> SaveNewVersionAsync(eTasksVersion version);
    }
}
