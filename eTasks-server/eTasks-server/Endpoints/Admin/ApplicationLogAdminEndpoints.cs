using eTasks_server.Core.BusinessLogicLayers.Interfaces;

namespace eTasks_server.Endpoints.Admin
{
    public static class ApplicationLogAdminEndpoints
    {
        public static void MapApplicationLogAdminEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/admin/logs")
                .WithTags("Application Logs")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription();

            group.MapGet("/{fileName}", async (
                string fileName,
                IApplicationLogAdminBLL applicationLogAdminBLL,
                CancellationToken cancellationToken) =>
            {
                var file = await applicationLogAdminBLL.DownloadFileAsync(fileName, cancellationToken);
                return Results.File(file.Content, file.ContentType, file.FileName);
            })
            .WithName("DownloadApplicationLog");
        }
    }
}
