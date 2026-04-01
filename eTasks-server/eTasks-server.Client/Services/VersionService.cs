using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Version;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace eTasks_server.Client.Services
{
    public class VersionService : BaseService, IVersionService
    {
        public VersionService(
            HttpClient httpClient,
            IDialogService dialogService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime)
            : base(httpClient, dialogService, navigationManager, jsRuntime) { }

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
