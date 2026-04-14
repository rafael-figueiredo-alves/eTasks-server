using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.Entities.Version;

namespace eTasks_server.Client.Services
{
    public class VersionService : IVersionService
    {
        private readonly IVersionBLL _versionBLL;

        public VersionService(IVersionBLL versionBLL)
        {
            _versionBLL = versionBLL;
        }

        public async Task<eTasksVersion> GetVersionAsync()
        {
            return await _versionBLL.GetVersionAsync();
        }

        public async Task<bool> SaveVersionAsync(eTasksVersion version)
        {
            return await _versionBLL.SaveNewVersionAsync(version);
        }
    }
}
