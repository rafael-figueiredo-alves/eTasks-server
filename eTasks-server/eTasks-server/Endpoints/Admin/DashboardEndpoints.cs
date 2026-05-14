using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using Scalar.AspNetCore;

namespace eTasks_server.Endpoints.Admin
{
    public static class DashboardEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// mapeia os endpoints relacionados ao dashboard, protegendo-os com a política de autorização "WebAdmin" e organizando-os sob o grupo "/api/v2/dashboard" com a tag "Dashboard". Esses endpoints fornecem métricas e informações relevantes para o painel de administração.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapDashboardEndpoints()
            {
                var group = app.MapGroup("/api/v2/dashboard")
                    .WithTags("Dashboard")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription();

                group.GetDashboardMetrics();

                return app;
            }
        }

        extension (RouteGroupBuilder group)
        {
            /// <summary>
            /// Retorna as métricas do dashboard, incluindo o total de usuários, novos usuários nos últimos 7 dias, tentativas de login falhadas hoje e tendências de login.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder GetDashboardMetrics()
            {
                group.MapGet("/", async (IDashboardBLL dashboardBLL) =>
                {
                    var response = await dashboardBLL.GetDashboardMetricsAsync();
                    return Results.Ok(response);
                })
                .WithName("GetDashboardMetrics");

                return group;
            }
        }

    }
}
