using eTasks_server.Models.DTOs.Users.Admin.Responses;

namespace eTasks_server.Client.Services
{
    public class UserLogsDrawerService
    {
        public AdminUserDTO? SelectedUser { get; private set; }
        public List<UserLoginLogDTO> SelectedUserLogs { get; private set; } = new();
        public bool IsOpen { get; private set; }

        public event Action? OnChange;

        public void Open(AdminUserDTO? user, List<UserLoginLogDTO> logs)
        {
            SelectedUser = user;
            SelectedUserLogs = logs;
            IsOpen = true;
            OnChange?.Invoke();
        }

        public void Close()
        {
            IsOpen = false;
            OnChange?.Invoke();
        }
    }
}
