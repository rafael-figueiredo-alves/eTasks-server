using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace eTasks_server.Endpoints
{
    public static class DashboardEndpoints
    {
        public static void MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/v2/dashboard")
                .WithTags("Dashboard")
                .RequireAuthorization("WebAdmin");

            group.MapGet("/", async (IDashboardBLL dashboardBLL) =>
            {
                var response = await dashboardBLL.GetDashboardMetricsAsync();
                return Results.Ok(response);
            })
            .WithName("GetDashboardMetrics")
            .WithOpenApi();
        }
    }
}
