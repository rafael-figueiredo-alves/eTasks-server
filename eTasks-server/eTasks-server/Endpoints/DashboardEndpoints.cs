using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using Scalar.AspNetCore;

namespace eTasks_server.Endpoints
{
    public static class DashboardEndpoints
    {
        public static void MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/api/v2/dashboard")
                .WithTags("Dashboard")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription();

            group.MapGet("/", async (IDashboardBLL dashboardBLL) =>
            {
                var response = await dashboardBLL.GetDashboardMetricsAsync();
                return Results.Ok(response);
            })
            .WithName("GetDashboardMetrics");
        }
    }
}
