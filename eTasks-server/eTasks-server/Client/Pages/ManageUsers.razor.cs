using eTasks_server.Client.Components;
using eTasks_server.Client.Services;
using eTasks_server.Client.Services.Extensions;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Admin.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class ManageUsersBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] protected IUserAdminService UserAdminService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] private UserLogsDrawerService LogsDrawerService { get; set; } = default!;
        #endregion

        #region Variáveis
        protected List<AdminUserDTO> Users = new();
        protected List<UserLoginLogDTO> SelectedUserLogs = new();
        protected AdminUserDTO? SelectedUser;
        protected bool IsLoading = true;
        protected bool IsDrawerOpen = false;
        protected string SearchString = "";
        #endregion

        #region Métodos
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
            await DialogService.ShowConfirm(                
                $"Deseja realmente {action} o usuário {user.Name}?",
                "Confirmação",
                EventCallback.Factory.Create(this, async () => await HandleBlockUser(user)));
        }

        private async Task HandleBlockUser(AdminUserDTO user)
        {
            if (await UserAdminService.ToggleBlockAsync(user.Uid))
            {
                Snackbar.Add($"Usuario {(user.IsBlocked ? "desbloqueado" : "bloqueado")} com sucesso!", Severity.Success);
                await LoadUsers();
            }
        }

        protected async Task ConfirmAccount(AdminUserDTO user)
        {
            await DialogService.ShowConfirm(                
                $"Deseja confirmar manualmente a conta de {user.Name}?",
                "Confirmar Conta",
                EventCallback.Factory.Create(this, async () => await HandleConfirmUserAccount(user)));
        }

        private async Task HandleConfirmUserAccount(AdminUserDTO user)
        {
            if (await UserAdminService.ConfirmAccountAsync(user.Uid))
            {
                Snackbar.Add("Conta confirmada com sucesso!", Severity.Success);
                await LoadUsers();
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
            await DialogService.ShowConfirm(                
                $"Enviar código de recuperação para {user.Email}?",
                "Enviar E-mail",
                EventCallback.Factory.Create(this, async () => await HandleSendPasswordResetEmail(user)));

        }

        private async Task HandleSendPasswordResetEmail(AdminUserDTO user)
        {
            if (await UserAdminService.SendPasswordResetEmailAsync(user.Uid))
            {
                Snackbar.Add("E-mail enviado com sucesso!", Severity.Success);
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

        protected async Task DeletePermanently(AdminUserDTO user)
        {
            await DialogService.ShowConfirm(                
                $"Esta ação é irreversível. A conta de {user.Name} ({user.Email}) e todos os seus dados serão removidos permanentemente do banco de dados. Deseja continuar?",
                "⚠️ Remover conta permanentemente",
                EventCallback.Factory.Create(this, async () => await HandleDeletePermanently(user)));
        }

        private async Task HandleDeletePermanently(AdminUserDTO user)
        {
            try
            {
                await UserAdminService.DeletePermanentlyAsync(user.Uid);
                Snackbar.Add($"Conta de {user.Name} removida permanentemente.", Severity.Success);
                await LoadUsers();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao remover conta: {ex.Message}", Severity.Error);
            }
        }

        protected async Task PurgeDeleted()
        {
            await DialogService.ShowConfirm(
                "Remover permanentemente todas as contas marcadas como excluídas? Esta ação não pode ser desfeita.",
                "⚠️ Purgar contas excluídas",
                EventCallback.Factory.Create(this, async () => await HandlePurgeDeleted()));
        }

        private async Task HandlePurgeDeleted()
        {
            try
            {
                var count = await UserAdminService.PurgeDeletedUsersAsync();
                var msg = count == 0
                    ? "Nenhuma conta excluída encontrada."
                    : $"{count} conta(s) removida(s) permanentemente.";
                Snackbar.Add(msg, count == 0 ? Severity.Info : Severity.Success);
                await LoadUsers();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erro ao purgar contas: {ex.Message}", Severity.Error);
            }
        }
        #endregion
    }
}
