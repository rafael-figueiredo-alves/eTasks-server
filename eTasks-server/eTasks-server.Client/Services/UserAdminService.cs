using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace eTasks_server.Client.Services
{
    public class UserAdminService : BaseService, IUserAdminService
    {
        public UserAdminService(
            HttpClient httpClient,
            IDialogService dialogService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime)
            : base(httpClient, dialogService, navigationManager, jsRuntime) { }

        public async Task<List<AdminUserDTO>> GetUsersAsync()
        {
            return await GetAsync<List<AdminUserDTO>>("users") ?? new List<AdminUserDTO>();
        }

        public async Task<bool> ToggleBlockAsync(Guid uid)
        {
            return await PatchAsync($"users/{uid}/block", new { });
        }

        public async Task<bool> SetPasswordAsync(Guid uid, string newPassword)
        {
            return await PatchAsync($"users/{uid}/password", new AdminSetPasswordRequest { NewPassword = newPassword });
        }

        public async Task<bool> ConfirmAccountAsync(Guid uid)
        {
            return await PatchAsync($"users/{uid}/confirm", new { });
        }

        public async Task<bool> SendPasswordResetEmailAsync(Guid uid)
        {
            return await PostAsync($"users/{uid}/send-reset", new { });
        }

        public async Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid)
        {
            return await GetAsync<List<UserLoginLogDTO>>($"users/{uid}/login-logs") ?? new List<UserLoginLogDTO>();
        }
    }
}
