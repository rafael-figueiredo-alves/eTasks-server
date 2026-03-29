using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Version;
using Scalar.AspNetCore;

namespace eTasks_server.Endpoints
{
    public static class VersionEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public async Task MapVersionEndpoints()
            {
                //Adiciona o endpoint para obter a versão atual do aplicativo
                await GetVersionAsync(app);

                //Adiciona o endpoint para salvar as edições da versão
                await ChangeVersionDetails(app);
            }

            private async Task GetVersionAsync()
            {
                app.MapGet("/version", async (AppDbContext dbContext) =>
                {
                    try
                    {
                        return Results.Ok(await VersionBLL.GetVersionAsync(dbContext));
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem($"Ocorreu um erro ao obter a versão: {ex.Message}");
                    }
                })
                   .WithTags("Versão da aplicação")
                   .WithName("Obter versão do eTasks")
                   .WithDescription("Retorna a versão atual do aplicativo, incluindo informações como número da versão, URL para download, etc.")
                   .WithSummary("Obtém a versão atual do aplicativo")
                   .WithDisplayName("Obter versão do eTasks")
                   .Produces(StatusCodes.Status200OK, typeof(eTasksVersion))
                   .Produces(StatusCodes.Status500InternalServerError);
            }

            private async Task ChangeVersionDetails()
            {
                app.MapPut("/version", async (AppDbContext dbContext, eTasksVersion version) =>
                {
                    if (await VersionBLL.SaveNewVersionAsync(dbContext, version))
                        return Results.Ok(version);
                    else
                        return Results.BadRequest("Não foi possível salvar edições da versão");
                })
                .WithTags("Versão da aplicação")
                .WithName("Salvar alterações na versão do App")
                .WithDisplayName("Salvar alterações na versão do App")
                .WithSummary("Salva as alterações na versão do aplicativo")
                .RequireAuthorization("Admin")
                .Produces(StatusCodes.Status200OK, typeof(eTasksVersion))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .WithDescription("Permite salvar as edições da versão do aplicativo. Requer autorização de administrador.");
            }
        }
    }
}
