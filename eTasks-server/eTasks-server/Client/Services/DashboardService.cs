using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Dashboard.Responses;

namespace eTasks_server.Client.Services
{
    public class DashboardService(IDashboardBLL _dashboardBLL, HttpClient _httpClient) : IDashboardService
    {
        public Task<DashboardResponse> GetDashboardDataAsync()
        {
            return _dashboardBLL.GetDashboardMetricsAsync();
        }

        public async Task<string> GetHealthStatusAsync()
        {
            try
            {
                // Como configuramos o BaseAddress no HttpClient "LocalApi", podemos usar o caminho relativo
                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress.ToString().Replace(_httpClient.BaseAddress.AbsolutePath, string.Empty)}/health");
                return response.IsSuccessStatusCode ? "Saudável" : "Doente";
            }
            catch
            {
                return "Doente";
            }
        }
    }
}
