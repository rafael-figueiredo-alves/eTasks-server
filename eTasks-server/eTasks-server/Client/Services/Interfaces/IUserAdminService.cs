using eTasks_server.Models.DTOs.Users.Admin.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IUserAdminService
    {
        Task<List<AdminUserDTO>> GetUsersAsync();
        Task<bool> ToggleBlockAsync(Guid uid);
        Task<bool> SetPasswordAsync(Guid uid, string newPassword);
        Task<bool> ConfirmAccountAsync(Guid uid);
        Task<bool> SendPasswordResetEmailAsync(Guid uid);
        Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid);
    }
}
