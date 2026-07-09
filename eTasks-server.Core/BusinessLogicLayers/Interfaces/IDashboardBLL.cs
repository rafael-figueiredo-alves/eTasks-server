using eTasks_server.Models.DTOs.Dashboard.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface do dashboard admin principal
    /// </summary>
    public interface IDashboardBLL
    {
        /// <summary>
        /// Obtem dados do dashboard
        /// </summary>
        /// <returns></returns>
        Task<DashboardResponse> GetDashboardMetricsAsync();
    }
}
