using eTasks_server.Models.Users;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IUserAdminBLL
    {
        Task<List<AdminUserDTO>> GetUsersAsync();
        Task<bool> ToggleBlockAsync(Guid uid);
        Task<bool> SetPasswordAsync(Guid uid, string newPassword);
        Task<bool> ConfirmAccountAsync(Guid uid);
        Task<bool> SendPasswordResetEmailAsync(Guid uid);
        Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid);
    }
}
