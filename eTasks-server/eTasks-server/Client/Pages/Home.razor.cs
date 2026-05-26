using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.DTOs.Dashboard.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class HomeBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] protected IDashboardService DashboardService { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        #endregion

        #region Variáveis
        protected DashboardResponse? DashboardData { get; set; }
        protected string HealthStatus { get; set; } = "Checking...";
        protected Color HealthColor { get; set; } = Color.Default;
        protected bool Loading { get; set; } = true;

        protected string[] ChartLabels { get; set; } = Array.Empty<string>();
        protected List<ChartSeries<double>> SeriesData { get; set; } = new();
        protected ChartOptions ChartOptions = new ChartOptions();

        private bool _dataLoaded = false;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            if (!_dataLoaded)
            {
                _dataLoaded = true;
                await LoadDataAsync();
            }
        }

        protected async Task LoadDataAsync()
        {
            Loading = true;
            try
            {
                DashboardData = await DashboardService.GetDashboardDataAsync();
                HealthStatus = await DashboardService.GetHealthStatusAsync();

                HealthColor = HealthStatus == "Saudável" ? Color.Success : Color.Error;

                if (DashboardData?.LoginTrends != null)
                {
                    PrepareChartData();
                }
            }
            catch (Exception)
            {
                HealthStatus = "Error";
                HealthColor = Color.Error;
            }
            finally
            {
                Loading = false;
                //StateHasChanged();
            }
        }

        private void PrepareChartData()
        {
            var successData = new List<double>();
            var failureData = new List<double>();
            var labels = new List<string>();

            foreach (var trend in DashboardData!.LoginTrends)
            {
                successData.Add(trend.SuccessCount);
                failureData.Add(trend.FailureCount);
                labels.Add(trend.Date.ToString("dd/MM"));
            }

            ChartLabels = labels.ToArray();
            SeriesData = new List<ChartSeries<double>>
            {
                new ChartSeries<double> { Name = "Sucessos", Data = successData.ToArray() },
                new ChartSeries<double> { Name = "Falhas", Data = failureData.ToArray() }
            };
        }

        protected void NavigateTo(string path, bool ForcarRender = false)
        {
            Navigation.NavigateTo(path, forceLoad: ForcarRender);
        }
        #endregion
    }
}
