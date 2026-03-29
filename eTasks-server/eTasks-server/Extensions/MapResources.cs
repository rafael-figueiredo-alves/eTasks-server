using eTasks_server.Components;
using eTasks_server.Models.Utils;

namespace eTasks_server.Extensions
{
    public static class MapResources
    {
        extension(WebApplication app)
        {
            public void MapResourcesEndpoints()
            {
                app.MapHealthChecks(Constants.HealthCheckEndpoint);

                app.MapStaticAssets();
                
                app.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode()
                    .AddInteractiveWebAssemblyRenderMode()
                    .AddAdditionalAssemblies(typeof(eTasks_server.Client._Imports).Assembly);
            }
        }
    }
}
