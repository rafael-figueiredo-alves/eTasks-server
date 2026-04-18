using eTasks_server.Models.DTOs.Dashboard.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardDataAsync();
        Task<string> GetHealthStatusAsync();
    }
}
