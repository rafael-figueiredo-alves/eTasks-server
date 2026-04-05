using eTasks_server.Client.Components;
using eTasks_server.Client.Layout;
using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Admin.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class ManageUsersBase : ComponentBase
    {
        [Inject] protected IUserAdminService UserAdminService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] private UserLogsDrawerService LogsDrawerService { get; set; } = default!;

        protected List<AdminUserDTO> Users = new();
        protected List<UserLoginLogDTO> SelectedUserLogs = new();
        protected AdminUserDTO? SelectedUser;
        protected bool IsLoading = true;
        protected bool IsDrawerOpen = false;
        protected string SearchString = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        protected async Task LoadUsers()
        {
            IsLoading = true;
            try
            {
                Users = await UserAdminService.GetUsersAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao carregar usuarios: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected bool FilterFunc(AdminUserDTO user)
        {
            if (string.IsNullOrWhiteSpace(SearchString)) return true;
            if (user.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)) return true;
            if (user.Email.Contains(SearchString, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        protected async Task ToggleBlock(AdminUserDTO user)
        {
            var action = user.IsBlocked ? "desbloquear" : "bloquear";
            bool? confirm = await DialogService.ShowMessageBoxAsync(
                "Confirmacao",
                $"Deseja realmente {action} o usuario {user.Name}?",
                yesText: "Sim", cancelText: "Nao");

            if (confirm == true)
            {
                if (await UserAdminService.ToggleBlockAsync(user.Uid))
                {
                    Snackbar.Add($"Usuario {(user.IsBlocked ? "desbloqueado" : "bloqueado")} com sucesso!", Severity.Success);
                    await LoadUsers();
                }
            }
        }

        protected async Task ConfirmAccount(AdminUserDTO user)
        {
            bool? confirm = await DialogService.ShowMessageBoxAsync(
                "Confirmar Conta",
                $"Deseja confirmar manualmente a conta de {user.Name}?",
                yesText: "Sim", cancelText: "Nao");

            if (confirm == true)
            {
                if (await UserAdminService.ConfirmAccountAsync(user.Uid))
                {
                    Snackbar.Add("Conta confirmada com sucesso!", Severity.Success);
                    await LoadUsers();
                }
            }
        }

        protected async Task ResetPassword(AdminUserDTO user)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            var dialog = await DialogService.ShowAsync<SetPasswordDialog>("Nova Senha", options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled && result.Data is string newPassword)
            {
                if (await UserAdminService.SetPasswordAsync(user.Uid, newPassword))
                {
                    Snackbar.Add("Senha alterada com sucesso!", Severity.Success);
                }
            }
        }

        protected async Task SendResetEmail(AdminUserDTO user)
        {
            bool? confirm = await DialogService.ShowMessageBoxAsync(
                "Enviar E-mail",
                $"Enviar codigo de recuperacao para {user.Email}?",
                yesText: "Enviar", cancelText: "Cancelar");

            if (confirm == true)
            {
                if (await UserAdminService.SendPasswordResetEmailAsync(user.Uid))
                {
                    Snackbar.Add("E-mail enviado com sucesso!", Severity.Success);
                }
            }
        }

        protected async Task ViewLogs(AdminUserDTO user)
        {
            LogsDrawerService.Open(user, await UserAdminService.GetLoginLogsAsync(user.Uid));
        }

        protected void CloseDrawer()
        {
            IsDrawerOpen = false;
            SelectedUser = null;
            SelectedUserLogs.Clear();
        }
    }
}
