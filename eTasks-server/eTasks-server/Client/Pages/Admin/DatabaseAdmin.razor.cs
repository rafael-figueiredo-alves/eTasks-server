using eTasks_server.Client.Services.Extensions;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Helpers;
using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class DatabaseAdminPage : ComponentBase
    {
        #region Serviços Injetados
        [Inject] private IDatabaseAdminService DatabaseAdminService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        #region Variáveis
        protected DatabaseOverviewResponse? _overview;
        protected string _script = string.Empty;
        protected string _adminKey = string.Empty;
        protected bool _confirmScriptExecution;
        protected bool _isLoading = true;
        protected bool _isBusy;
        protected string _statusMessage = string.Empty;
        protected Severity _statusSeverity = Severity.Info;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            await ReloadAsync();
        }

        protected async Task ReloadAsync()
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                _isLoading = true;
                _overview = await DatabaseAdminService.GetOverviewAsync();
                SetStatus($"Informações carregadas em {_overview.GeneratedAt:dd/MM/yyyy HH:mm:ss}.", Severity.Info);
            }, "Erro ao carregar informações do banco.", Snackbar, value => _isBusy = value, SetStatus);

            _isLoading = false;
        }

        protected async Task ExecuteScriptAsync()
        {
            if (!_confirmScriptExecution)
            {
                SetStatus("Confirme a revisão do script antes de executar.", Severity.Warning);
                return;
            }

            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                var response = await DatabaseAdminService.ExecuteScriptAsync(new DatabaseScriptExecutionRequest
                {
                    Script = _script
                });

                SetStatus(response.Message, Severity.Success);
                Snackbar.Add(response.Message, Severity.Success);
                _confirmScriptExecution = false;
                await ReloadAsync();
            }, "Erro ao executar script SQL.", Snackbar, value => _isBusy = value, SetStatus);
        }

        protected void ClearScript()
        {
            _script = string.Empty;
            _confirmScriptExecution = false;
        }

        protected async Task ClearDatabaseAsync()
        {
            await DialogService.ShowConfirm("Deseja remover os dados do MySQL preservando apenas os usuários administradores?",
                "Limpar base MySQL", EventCallback.Factory.Create(this, async() => await HandleCleanDatabase()));
        }

        private async Task HandleCleanDatabase()
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                var response = await DatabaseAdminService.ClearDatabaseAsync(_adminKey);
                _adminKey = string.Empty;
                SetStatus(response.Message, Severity.Success);
                Snackbar.Add(response.Message, Severity.Success);
                await ReloadAsync();
            }, "Erro ao limpar base MySQL.", Snackbar, value => _isBusy = value, SetStatus);
        }

        public static string FormatBytes(long bytes)
        {
            string[] units = ["B", "KB", "MB", "GB", "TB"];
            var value = (double)bytes;
            var unit = 0;

            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }

            return $"{value:0.##} {units[unit]}";
        }

        private void SetStatus(string message, Severity severity)
        {
            _statusMessage = message;
            _statusSeverity = severity;
        }
        #endregion
    }

    internal static class DatabaseOverviewResponseExtensions
    {
        public static string DataLengthBytesFormatted(this DatabaseOverviewResponse response)
            => DatabaseAdminPage.FormatBytes(response.DataLengthBytes);

        public static string IndexLengthBytesFormatted(this DatabaseOverviewResponse response)
            => DatabaseAdminPage.FormatBytes(response.IndexLengthBytes);
    }
}
