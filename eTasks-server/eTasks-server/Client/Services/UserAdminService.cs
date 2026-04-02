using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Users;

namespace eTasks_server.Client.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly IUserAdminBLL _userAdminBLL;

        public UserAdminService(IUserAdminBLL userAdminBLL)
        {
            _userAdminBLL = userAdminBLL;
        }

        public Task<List<AdminUserDTO>> GetUsersAsync()
        {
            return _userAdminBLL.GetUsersAsync();
        }

        public Task<bool> ToggleBlockAsync(Guid uid)
        {
            return _userAdminBLL.ToggleBlockAsync(uid);
        }

        public Task<bool> SetPasswordAsync(Guid uid, string newPassword)
        {
            return _userAdminBLL.SetPasswordAsync(uid, newPassword);
        }

        public Task<bool> ConfirmAccountAsync(Guid uid)
        {
            return _userAdminBLL.ConfirmAccountAsync(uid);
        }

        public Task<bool> SendPasswordResetEmailAsync(Guid uid)
        {
            return _userAdminBLL.SendPasswordResetEmailAsync(uid);
        }

        public Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid)
        {
            return _userAdminBLL.GetLoginLogsAsync(uid);
        }
    }
}
