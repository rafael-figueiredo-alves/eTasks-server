using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Helpers;
using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Users.Admin.Responses;
using eTasks_server.Models.Enums.Notifications;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class SendNotificationsPage : ComponentBase
    {
        #region Serviços injetados
        [Inject] private IAdminNotificationService AdminNotificationService { get; set; } = default!;
        [Inject] private IUserAdminService UserAdminService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        #region Variáveis
        protected SendAdminNotificationRequest _request = new();
        protected IReadOnlyList<AdminUserDTO> _users = [];
        protected IReadOnlyList<Guid> _selectedUserUids = [];
        protected MudForm? _form;
        protected bool _isBusy;
        protected string _statusMessage = string.Empty;
        protected Severity _statusSeverity = Severity.Info;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            await ReloadUsersAsync();
        }

        protected async Task ReloadUsersAsync()
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                _users = await UserAdminService.GetUsersAsync();
                SetStatus($"{_users.Count} usuário(s) comum(ns) disponível(is) para seleção individual.", Severity.Info);
            }, "Erro ao carregar usuários.", Snackbar, value => _isBusy = value, SetStatus);
        }

        protected void OnSelectedUsersChanged(IEnumerable<Guid> values)
        {
            _selectedUserUids = values.ToList();
            _request.UserUids = _selectedUserUids.ToList();
        }

        protected string GetUserLabel(Guid userUid)
        {
            var user = _users.FirstOrDefault(x => x.Uid == userUid);
            return user is null ? userUid.ToString() : $"{user.Name} - {user.Email}";
        }

        protected async Task SendAsync()
        {
            if (_form is null)
            {
                return;
            }

            await _form.ValidateAsync();
            if (!_form.IsValid)
            {
                return;
            }

            if (_request.TargetType == NotificationTargetType.SelectedUsers)
            {
                _request.UserUids = _selectedUserUids.ToList();
            }

            await ExecuteBusyAsync(async () =>
            {
                var response = await AdminNotificationService.SendAsync(_request);
                var message = $"Notificação enviada para {response.RecipientCount} destinatário(s). Dispositivos registrados: {response.RegisteredDeviceCount}.";
                SetStatus(message, Severity.Success);
                Snackbar.Add(message, Severity.Success);
                Clear();
            }, "Erro ao enviar notificação.");
        }

        protected void Clear()
        {
            _request = new SendAdminNotificationRequest();
            _selectedUserUids = [];
        }

        private void SetStatus(string message, Severity severity)
        {
            _statusMessage = message;
            _statusSeverity = severity;
        }
        #endregion
    }
}
