using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Version;
using MudBlazor;

namespace eTasks_server.Client.Services
{
    public class VersionService : BaseService, IVersionService
    {
        public VersionService(HttpClient httpClient, IDialogService dialogService) : base(httpClient, dialogService) { }

        public async Task<eTasksVersion> GetVersionAsync()
        {
            return await GetAsync<eTasksVersion>("version") ?? new eTasksVersion();
        }

        public async Task<bool> SaveVersionAsync(eTasksVersion version)
        {
            return await PutAsync("version", version);
        }
    }
}
