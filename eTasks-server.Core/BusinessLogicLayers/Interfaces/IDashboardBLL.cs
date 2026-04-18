using eTasks_server.Models.DTOs.Dashboard.Responses;
using System.Threading.Tasks;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IDashboardBLL
    {
        Task<DashboardResponse> GetDashboardMetricsAsync();
    }
}
