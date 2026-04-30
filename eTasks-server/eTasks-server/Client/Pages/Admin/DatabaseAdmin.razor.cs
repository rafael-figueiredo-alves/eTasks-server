using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class DatabaseAdminPage : ComponentBase
    {
        [Inject] private IDatabaseAdminService DatabaseAdminService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected DatabaseOverviewResponse? _overview;
        protected string _script = string.Empty;
        protected string _adminKey = string.Empty;
        protected bool _confirmScriptExecution;
        protected bool _isLoading = true;
        protected bool _isBusy;
        protected string _statusMessage = string.Empty;
        protected Severity _statusSeverity = Severity.Info;

        protected override async Task OnInitializedAsync()
        {
            await ReloadAsync();
        }

        protected async Task ReloadAsync()
        {
            await ExecuteBusyAsync(async () =>
            {
                _isLoading = true;
                _overview = await DatabaseAdminService.GetOverviewAsync();
                SetStatus($"Informacoes carregadas em {_overview.GeneratedAt:dd/MM/yyyy HH:mm:ss}.", Severity.Info);
            }, "Erro ao carregar informacoes do banco.");

            _isLoading = false;
        }

        protected async Task ExecuteScriptAsync()
        {
            if (!_confirmScriptExecution)
            {
                SetStatus("Confirme a revisao do script antes de executar.", Severity.Warning);
                return;
            }

            await ExecuteBusyAsync(async () =>
            {
                var response = await DatabaseAdminService.ExecuteScriptAsync(new DatabaseScriptExecutionRequest
                {
                    Script = _script
                });

                SetStatus(response.Message, Severity.Success);
                Snackbar.Add(response.Message, Severity.Success);
                _confirmScriptExecution = false;
                await ReloadAsync();
            }, "Erro ao executar script SQL.");
        }

        protected void ClearScript()
        {
            _script = string.Empty;
            _confirmScriptExecution = false;
        }

        protected async Task ClearDatabaseAsync()
        {
            var confirmed = await DialogService.ShowMessageBoxAsync(
                "Limpar base MySQL",
                "Deseja remover os dados do MySQL preservando apenas os usuarios administradores?",
                yesText: "Limpar",
                cancelText: "Cancelar");

            if (confirmed != true)
            {
                return;
            }

            await ExecuteBusyAsync(async () =>
            {
                var response = await DatabaseAdminService.ClearDatabaseAsync(_adminKey);
                _adminKey = string.Empty;
                SetStatus(response.Message, Severity.Success);
                Snackbar.Add(response.Message, Severity.Success);
                await ReloadAsync();
            }, "Erro ao limpar base MySQL.");
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

        private async Task ExecuteBusyAsync(Func<Task> action, string defaultErrorMessage)
        {
            try
            {
                _isBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                var message = string.IsNullOrWhiteSpace(ex.Message) ? defaultErrorMessage : ex.Message;
                SetStatus(message, Severity.Error);
                Snackbar.Add(message, Severity.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void SetStatus(string message, Severity severity)
        {
            _statusMessage = message;
            _statusSeverity = severity;
        }
    }

    internal static class DatabaseOverviewResponseExtensions
    {
        public static string DataLengthBytesFormatted(this DatabaseOverviewResponse response)
            => DatabaseAdminPage.FormatBytes(response.DataLengthBytes);

        public static string IndexLengthBytesFormatted(this DatabaseOverviewResponse response)
            => DatabaseAdminPage.FormatBytes(response.IndexLengthBytes);
    }
}
