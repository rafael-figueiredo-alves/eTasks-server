using eTasks_server.Client.Services.Extensions;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Helpers;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class ApplicationLogsPage : ComponentBase
    {
        #region Serviços Injetados
        [Inject] private IApplicationLogAdminService ApplicationLogAdminService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        #region Variáveis
        protected IReadOnlyList<LogFileSummaryResponse> _files = [];
        protected LogFileContentResponse? _selectedFile;
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
            await ExecuteBusyAsync(async () =>
            {
                _isLoading = true;
                _files = await ApplicationLogAdminService.GetFilesAsync();
                SetStatus($"{_files.Count} arquivo(s) de log encontrado(s).", Severity.Info);
            }, "Erro ao carregar arquivos de log.");

            _isLoading = false;
        }

        protected async Task OpenFileAsync(string fileName)
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                _selectedFile = await ApplicationLogAdminService.ReadFileAsync(fileName);
                SetStatus($"Arquivo {_selectedFile.FileName} carregado.", Severity.Success);
            }, "Erro ao abrir arquivo de log.", Snackbar, value => _isBusy = value, SetStatus);
        }

        protected async Task DeleteFileAsync(string fileName)
        {
            await DialogService.ShowConfirm($"Deseja apagar o arquivo {fileName}?", "Apagar log", EventCallback.Factory.Create(this, async () => await HandleDeleteFileAsync(fileName)));
        }

        private async Task HandleDeleteFileAsync(string fileName)
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                await ApplicationLogAdminService.DeleteFileAsync(fileName);
                if (_selectedFile?.FileName == fileName)
                {
                    _selectedFile = null;
                }

                Snackbar.Add("Arquivo de log apagado.", Severity.Success);
                await ReloadAsync();
            }, "Erro ao apagar arquivo de log.", Snackbar, value => _isBusy = value, SetStatus);
        }

        protected static string GetDownloadUrl(string fileName)
            => $"/api/v2/admin/logs/{Uri.EscapeDataString(fileName)}";

        protected static string FormatBytes(long bytes)
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
}
