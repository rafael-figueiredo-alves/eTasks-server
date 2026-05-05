using eTasks_server.Components;
using eTasks_server.Models.Utils;

namespace eTasks_server.Extensions
{
    /// <summary>
    /// Classe de extensão para mapear os recursos da aplicação, como endpoints de health check, recursos estáticos e Razor Components (Blazor Server).
    /// </summary>
    public static class MapResources
    {
        extension(WebApplication app)
        {
            // Método de extensão para mapear os recursos da aplicação
            public void MapResourcesEndpoints()
            {
                //Mapea os endpoints de health check
                app.MapHealthChecks(Constants.HealthCheckEndpoint);

                //Mapea os recursos estáticos (CSS, JS, imagens, etc.)
                app.MapStaticAssets();

                //Mapea os endpoints de Razor Components (Blazor Server)
                app.MapRazorComponents<App>()
                    .AddInteractiveServerRenderMode();
            }
        }
    }
}
