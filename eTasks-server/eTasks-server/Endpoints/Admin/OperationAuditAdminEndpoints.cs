using eTasks_server.Core.BusinessLogicLayers.Interfaces;

namespace eTasks_server.Endpoints.Admin
{
    public static class OperationAuditAdminEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados à administração de auditoria de operações, incluindo a geração de backup dos logs de auditoria.
            /// </summary>
            /// <param name="endpoints"></param>
            /// <returns></returns>
            public IEndpointRouteBuilder MapOperationAuditAdminEndpoints()
            {
                var group = app.MapGroup("/admin/operation-audit")
                    .WithTags("Operation Audit Admin")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription();

                group.MakeAuditBackup();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Método para gerar um backup dos logs de auditoria de operações. Este endpoint é protegido por autorização e retorna um arquivo contendo os dados de auditoria, que pode ser baixado pelo administrador.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder MakeAuditBackup()
            {
                group.MapGet("/backup", async (IOperationAuditAdminBLL operationAuditAdminBLL, CancellationToken cancellationToken) =>
                {
                    var backup = await operationAuditAdminBLL.GenerateBackupAsync(cancellationToken);
                    return Results.File(backup.Content, backup.ContentType, backup.FileName);
                })
                .WithName("DownloadOperationAuditBackup");

                return group;
            }
        }
    }
}
