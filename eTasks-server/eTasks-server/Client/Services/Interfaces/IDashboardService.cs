using eTasks_server.Models.DTOs.Dashboard.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface de serviços do Dashboard principal
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Resgata informações para o dashboard principal
        /// </summary>
        /// <returns>Dados do dashboard</returns>
        Task<DashboardResponse> GetDashboardDataAsync();

        /// <summary>
        /// Obtem saúde da conexão com o banco de dados
        /// </summary>
        /// <returns>Texto do status: Saudável ou doente</returns>
        Task<string> GetHealthStatusAsync();
    }
}
