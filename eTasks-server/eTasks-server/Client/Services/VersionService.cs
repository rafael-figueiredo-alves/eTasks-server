using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;
using eTasks_server.Models.Version;

namespace eTasks_server.Client.Services
{
    public class VersionService : IVersionService
    {
        private readonly AppDbContext _dbContext;

        public VersionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<eTasksVersion> GetVersionAsync()
        {
            return await VersionBLL.GetVersionAsync(_dbContext);
        }

        public async Task<bool> SaveVersionAsync(eTasksVersion version)
        {
            return await VersionBLL.SaveNewVersionAsync(_dbContext, version);
        }
    }
}
