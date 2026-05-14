using eTasks_server.Core.BusinessLogicLayers.Interfaces;

namespace eTasks_server.Endpoints.Admin
{
    public static class DatabaseAdminEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados à administração do banco de dados, como backup e execução de scripts. Esses endpoints são protegidos por autorização e destinados a administradores do sistema.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapDatabaseAdminEndpoints()
            {
                var group = app.MapGroup("/admin/database")
                    .WithTags("Database Admin")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription();

                group.MakeDatabaseBackup();

                return app;
            }            
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Método de extensão para mapear o endpoint de backup do banco de dados. Este endpoint permite que administradores façam o download de um backup do banco de dados, retornando um arquivo contendo os dados do backup. O endpoint é protegido por autorização e é destinado a administradores do sistema.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder MakeDatabaseBackup()
            {
                group.MapGet("/backup", async (IDatabaseAdminBLL databaseAdminBLL, CancellationToken cancellationToken) =>
                {
                    var backup = await databaseAdminBLL.GenerateBackupAsync(cancellationToken);
                    return Results.File(backup.Content, backup.ContentType, backup.FileName);
                })
                .WithName("DownloadDatabaseBackup");

                return group;
            }
        }
    }
}
