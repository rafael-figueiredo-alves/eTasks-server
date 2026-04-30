using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Dashboard.Responses;

namespace eTasks_server.Client.Services
{
    public class DashboardService(IServiceScopeFactory scopeFactory, HttpClient httpClient) : IDashboardService
    {
        public async Task<DashboardResponse> GetDashboardDataAsync()
        {
            using var scope = scopeFactory.CreateScope();
            var dashboardBLL = scope.ServiceProvider.GetRequiredService<IDashboardBLL>();
            return await dashboardBLL.GetDashboardMetricsAsync();
        }

        public async Task<string> GetHealthStatusAsync()
        {
            try
            {
                var baseAddress = httpClient.BaseAddress
                    ?? throw new InvalidOperationException("BaseAddress do HttpClient LocalApi nao configurado.");
                var healthUrl = $"{baseAddress.ToString().Replace(baseAddress.AbsolutePath, string.Empty)}/health";
                var response = await httpClient.GetAsync(healthUrl);
                return response.IsSuccessStatusCode ? "Saudável" : "Doente";
            }
            catch
            {
                return "Doente";
            }
        }
    }
}
