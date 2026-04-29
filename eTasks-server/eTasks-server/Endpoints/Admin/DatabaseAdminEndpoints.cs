using eTasks_server.Core.BusinessLogicLayers.Interfaces;

namespace eTasks_server.Endpoints.Admin
{
    public static class DatabaseAdminEndpoints
    {
        public static void MapDatabaseAdminEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/admin/database")
                .WithTags("Database Admin")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription();

            group.MapGet("/backup", async (IDatabaseAdminBLL databaseAdminBLL, CancellationToken cancellationToken) =>
            {
                var backup = await databaseAdminBLL.GenerateBackupAsync(cancellationToken);
                return Results.File(backup.Content, backup.ContentType, backup.FileName);
            })
            .WithName("DownloadDatabaseBackup");
        }
    }
}
