using eTasks_server.Endpoints.Admin;
using eTasks_server.Endpoints.AI;
using eTasks_server.Endpoints.API_Resourcers.Goals;
using eTasks_server.Endpoints.API_Resourcers.Finances;
using eTasks_server.Endpoints.API_Resourcers.Notes;
using eTasks_server.Endpoints.API_Resourcers.Readings;
using eTasks_server.Endpoints.API_Resourcers.Shopping;
using eTasks_server.Endpoints.API_Resourcers.Tasks;
using eTasks_server.Endpoints.Auth;
using eTasks_server.Endpoints.Usuarios;
using eTasks_server.Endpoints.Utils;
using eTasks_server.Endpoints.Version;

namespace eTasks_server.Endpoints
{
    public static class EndpointsEntry
    {
        extension(WebApplication app)
        {
            public async Task AddAPIEndpoints()
            {
                var API_V2 = app.MapGroup("/api")
                                    .MapGroup("/v2");

                API_V2.MapVersionEndpoints();
                API_V2.MapUtilsEndpoints();
                API_V2.MapAuthEndpoints();
                API_V2.MapWebAuthEndpoints();
                API_V2.MapAiAssistantEndpoints();
                API_V2.MapUserAdminEndpoints();
                API_V2.MapBonusAdminEndpoints();
                API_V2.MapUsuariosEndpoints();
                API_V2.MapTasksEndpoints();
                API_V2.MapGoalsEndpoints();
                API_V2.MapNotesEndpoints();
                API_V2.MapReadingsEndpoints();
                API_V2.MapShoppingListsEndpoints();
                API_V2.MapFinancesEndpoints();
                API_V2.MapDashboardEndpoints();
            }
        }
    }
}
