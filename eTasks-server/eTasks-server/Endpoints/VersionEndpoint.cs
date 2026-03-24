using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;
using eTasks_server.Models.Version;

namespace eTasks_server.Endpoints
{
    public static class VersionEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public async Task MapVersionEndpoints()
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
                   .WithTags("Version")
                   .WithName("GetVersion")
                   .WithDescription("Retorna a versão atual do aplicativo, incluindo informações como número da versão, URL para download, etc.")
                   .WithSummary("Obtém a versão atual do aplicativo")
                   .Produces(StatusCodes.Status200OK, typeof(eTasksVersion));

                app.MapPut("/version", async (AppDbContext dbContext, eTasksVersion version) => {
                    if (await VersionBLL.SaveNewVersionAsync(dbContext, version))
                        return Results.Ok(version);                    
                    else 
                        return Results.BadRequest("Não foi possível salvar edições da versão");
                })
                   .WithTags("Version")
                   .WithName("SaveNewVersion");
            }
        }
    }
}
