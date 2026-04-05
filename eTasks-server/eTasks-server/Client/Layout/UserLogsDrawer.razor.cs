using eTasks_server.Client.Services;
using eTasks_server.Models.DTOs.Users.Admin.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Layout
{
    public class UserLogsDrawerBase : ComponentBase, IDisposable
    {
        [Inject] protected UserLogsDrawerService DrawerService { get; set; } = default!;

        protected Anchor _anchor { get; set; }

        protected bool IsDrawerOpen
        {
            get { _anchor = Anchor.End; return DrawerService.IsOpen; }
            set { if (!value) { _anchor = Anchor.Start; DrawerService.Close(); } }
        }

        protected AdminUserDTO? SelectedUser => DrawerService.SelectedUser;
        protected List<UserLoginLogDTO> SelectedUserLogs => DrawerService.SelectedUserLogs;

        protected override void OnInitialized()
        {
            DrawerService.OnChange += OnDrawerStateChanged;
        }

        private void OnDrawerStateChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            DrawerService.OnChange -= OnDrawerStateChanged;
        }
    }
}
