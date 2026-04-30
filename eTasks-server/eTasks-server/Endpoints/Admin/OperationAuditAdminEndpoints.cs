using eTasks_server.Core.BusinessLogicLayers.Interfaces;

namespace eTasks_server.Endpoints.Admin
{
    public static class OperationAuditAdminEndpoints
    {
        public static void MapOperationAuditAdminEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("/admin/operation-audit")
                .WithTags("Operation Audit Admin")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription();

            group.MapGet("/backup", async (IOperationAuditAdminBLL operationAuditAdminBLL, CancellationToken cancellationToken) =>
            {
                var backup = await operationAuditAdminBLL.GenerateBackupAsync(cancellationToken);
                return Results.File(backup.Content, backup.ContentType, backup.FileName);
            })
            .WithName("DownloadOperationAuditBackup");
        }
    }
}
