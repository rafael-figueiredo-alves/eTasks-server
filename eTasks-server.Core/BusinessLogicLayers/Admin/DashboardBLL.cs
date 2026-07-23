using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Dashboard.Responses;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para consolidacao dos indicadores do painel administrativo.
    /// </summary>
    public class DashboardBLL(AppDbContext context, ILogger<IDashboardBLL> logger) : BaseBLL<IDashboardBLL>(context, logger), IDashboardBLL
    {
        /// <summary>
        /// Retorna os principais indicadores do dashboard administrativo.
        /// </summary>
        /// <returns>Resumo com metricas de usuarios e logins.</returns>
        public async Task<DashboardResponse> GetDashboardMetricsAsync()
        {
            var now = SaoPauloDateTime.Now();
            var sevenDaysAgo = now.AddDays(-7).Date;
            var today = now.Date;

            var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            var newUsersLast7Days = await _context.Users.CountAsync(u => !u.IsDeleted && u.CreatedAt >= sevenDaysAgo);
            var failedLoginsToday = await _context.LoginLogs.CountAsync(l => l.Status != "Success" && l.CreatedAt >= today);

            var logs = await _context.LoginLogs
                .Where(l => l.CreatedAt >= sevenDaysAgo)
                .Select(l => new { l.CreatedAt, l.Status })
                .ToListAsync();

            var trends = logs
                .GroupBy(l => l.CreatedAt.Date)
                .Select(g => new LoginTrendItem
                {
                    Date = g.Key,
                    SuccessCount = g.Count(l => l.Status == "Success"),
                    FailureCount = g.Count(l => l.Status != "Success")
                })
                .OrderBy(t => t.Date)
                .ToList();

            // Garantir que todos os últimos 7 dias estejam presentes na lista, mesmo sem logs
            var finalTrends = new List<LoginTrendItem>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var existing = trends.FirstOrDefault(t => t.Date == date);
                if (existing != null)
                {
                    finalTrends.Add(existing);
                }
                else
                {
                    finalTrends.Add(new LoginTrendItem { Date = date, SuccessCount = 0, FailureCount = 0 });
                }
            }

            return new DashboardResponse
            {
                TotalUsers = totalUsers,
                NewUsersLast7Days = newUsersLast7Days,
                FailedLoginsToday = failedLoginsToday,
                LoginTrends = finalTrends
            };
        }
    }
}
