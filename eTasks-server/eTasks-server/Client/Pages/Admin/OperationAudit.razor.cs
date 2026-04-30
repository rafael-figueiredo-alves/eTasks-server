using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.OperationAudit.Requests;
using eTasks_server.Models.DTOs.OperationAudit.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class OperationAuditPage : ComponentBase
    {
        [Inject] private IOperationAuditAdminService OperationAuditAdminService { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        protected OperationAuditDashboardResponse? _dashboard;
        protected OperationAuditLogPageResponse? _entries;
        protected OperationAuditLogQueryRequest _query = new();
        protected string _adminKey = string.Empty;
        protected bool _isLoading = true;
        protected bool _isBusy;
        protected string _statusMessage = string.Empty;
        protected Severity _statusSeverity = Severity.Info;
        protected string[] UsageLabels { get; set; } = [];
        protected List<ChartSeries<double>> UsageSeries { get; set; } = [];
        protected ChartOptions UsageChartOptions { get; set; } = new();
        protected bool _canGoPrevious => _entries is not null && _entries.PageIndex > 0;
        protected bool _canGoNext => _entries is not null && _entries.PageIndex + 1 < _entries.TotalPages;

        protected override async Task OnInitializedAsync()
        {
            await ReloadAsync();
        }

        protected async Task ReloadAsync()
        {
            await ExecuteBusyAsync(async () =>
            {
                _isLoading = true;
                _dashboard = await OperationAuditAdminService.GetDashboardAsync();
                BuildUsageChart();

                if (_dashboard.MongoAuditEnabled && _dashboard.IsConfigured)
                {
                    _entries = await OperationAuditAdminService.GetEntriesAsync(_query);
                    SetStatus($"{_entries.TotalItems} entrada(s) de auditoria encontradas.", Severity.Info);
                }
                else
                {
                    _entries = null;
                    SetStatus("Auditoria MongoDB desabilitada ou incompleta.", Severity.Warning);
                }
            }, "Erro ao carregar auditoria operacional.");

            _isLoading = false;
        }

        protected async Task ClearMongoAsync()
        {
            var confirmed = await DialogService.ShowMessageBoxAsync(
                "Limpar auditoria MongoDB",
                "Deseja remover todas as entradas de auditoria operacional do MongoDB?",
                yesText: "Limpar",
                cancelText: "Cancelar");

            if (confirmed != true)
            {
                return;
            }

            await ExecuteBusyAsync(async () =>
            {
                var deleted = await OperationAuditAdminService.ClearAsync(_adminKey);
                _adminKey = string.Empty;
                Snackbar.Add($"{deleted} entrada(s) removidas da auditoria MongoDB.", Severity.Success);
                await ReloadAsync();
            }, "Erro ao limpar auditoria MongoDB.");
        }

        protected async Task ApplyFiltersAsync()
        {
            _query.PageIndex = 0;
            await LoadEntriesAsync();
        }

        protected async Task ClearFiltersAsync()
        {
            _query = new OperationAuditLogQueryRequest { PageSize = _query.PageSize };
            await LoadEntriesAsync();
        }

        protected async Task PreviousPageAsync()
        {
            if (!_canGoPrevious)
            {
                return;
            }

            _query.PageIndex--;
            await LoadEntriesAsync();
        }

        protected async Task NextPageAsync()
        {
            if (!_canGoNext)
            {
                return;
            }

            _query.PageIndex++;
            await LoadEntriesAsync();
        }

        protected async Task ChangePageSizeAsync(int pageSize)
        {
            _query.PageSize = pageSize;
            _query.PageIndex = 0;
            await LoadEntriesAsync();
        }

        protected static string FormatDateTime(DateTime? value)
            => value.HasValue ? value.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss") : "-";

        protected static string FormatDuration(double durationMs)
            => durationMs >= 1000 ? $"{durationMs / 1000:0.##} s" : $"{durationMs:0.##} ms";

        protected static Color GetStatusColor(int statusCode)
            => statusCode switch
            {
                >= 500 => Color.Error,
                >= 400 => Color.Warning,
                >= 300 => Color.Info,
                _ => Color.Success
            };

        private async Task LoadEntriesAsync()
        {
            await ExecuteBusyAsync(async () =>
            {
                _entries = await OperationAuditAdminService.GetEntriesAsync(_query);
                SetStatus($"{_entries.TotalItems} entrada(s) de auditoria encontradas.", Severity.Info);
            }, "Erro ao carregar entradas de auditoria.");
        }

        private void BuildUsageChart()
        {
            if (_dashboard?.UsageTrend.Count > 0)
            {
                UsageLabels = _dashboard.UsageTrend.Select(x => x.Label).ToArray();
                UsageSeries =
                [
                    new ChartSeries<double>
                    {
                        Name = "Requisições",
                        Data = _dashboard.UsageTrend.Select(x => (double)x.TotalCount).ToArray()
                    },
                    new ChartSeries<double>
                    {
                        Name = "Erros",
                        Data = _dashboard.UsageTrend.Select(x => (double)x.ErrorCount).ToArray()
                    }
                ];
                return;
            }

            UsageLabels = [];
            UsageSeries = [];
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
}
