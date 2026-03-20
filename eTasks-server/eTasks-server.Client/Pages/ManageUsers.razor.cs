using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Client.Components;
using eTasks_server.Models.Users;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class ManageUsersBase : ComponentBase
    {
        [Inject] protected IUserAdminService UserAdminService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;

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
                Snackbar.Add($"Erro ao carregar usuários: {ex.Message}", Severity.Error);
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
                "Confirmação", 
                $"Deseja realmente {action} o usuário {user.Name}?", 
                yesText: "Sim", cancelText: "Não");

            if (confirm == true)
            {
                if (await UserAdminService.ToggleBlockAsync(user.Uid))
                {
                    Snackbar.Add($"Usuário {(user.IsBlocked ? "desbloqueado" : "bloqueado")} com sucesso!", Severity.Success);
                    await LoadUsers();
                }
            }
        }

        protected async Task ConfirmAccount(AdminUserDTO user)
        {
            bool? confirm = await DialogService.ShowMessageBoxAsync(
                "Confirmar Conta", 
                $"Deseja confirmar manualmente a conta de {user.Name}?", 
                yesText: "Sim", cancelText: "Não");

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
            var parameters = new DialogParameters();
            
            // Usando um prompt simples do MudBlazor para capturar a nova senha
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
                $"Enviar código de recuperação para {user.Email}?", 
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
            SelectedUser = user;
            SelectedUserLogs = await UserAdminService.GetLoginLogsAsync(user.Uid);
            IsDrawerOpen = true;
        }

        protected void CloseDrawer()
        {
            IsDrawerOpen = false;
            SelectedUser = null;
            SelectedUserLogs.Clear();
        }
    }
}
