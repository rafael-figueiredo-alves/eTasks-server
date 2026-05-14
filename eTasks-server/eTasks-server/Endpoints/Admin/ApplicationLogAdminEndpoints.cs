using eTasks_server.Core.BusinessLogicLayers.Interfaces;

namespace eTasks_server.Endpoints.Admin
{
    public static class ApplicationLogAdminEndpoints
    {
        extension (IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados à administração dos logs de aplicação, permitindo o download dos arquivos de log para análise e monitoramento.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapApplicationLogAdminEndpoints()
            {
                var group = app.MapGroup("/admin/logs")
                    .WithTags("Application Logs")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription();

                group.DownloadLogFiles();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Método de extensão para mapear o endpoint de download dos arquivos de log de aplicação, permitindo que os administradores façam o download dos arquivos para análise e monitoramento.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder DownloadLogFiles()
            {
                group.MapGet("/{fileName}", async (
                    string fileName,
                    IApplicationLogAdminBLL applicationLogAdminBLL,
                    CancellationToken cancellationToken) =>
                {
                    var file = await applicationLogAdminBLL.DownloadFileAsync(fileName, cancellationToken);

                    return Results.File(file.Content, file.ContentType, file.FileName);
                })
                .WithName("DownloadApplicationLog");

                return group;
            }
        }
    }
}
