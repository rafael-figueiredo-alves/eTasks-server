using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;
using eTasks_server.Core.Models;

namespace eTasks_server.Endpoints
{
    public static class VersionEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public async Task MapVersionEndpoints()
            {
                app.MapGet("/version", async (AppDbContext dbContext) => Results.Ok(await VersionBLL.GetVersionAsync(dbContext)))
                   .WithTags("Version")
                   .WithName("GetVersion");

                app.MapPost("/version", async (AppDbContext dbContext, eTasksVersion version) => {
                    if (await VersionBLL.SaveNewVersionAsync(dbContext, version))
                        Results.Ok(version);
                    else Results.BadRequest("Não foi possível salvar edições da versão");
                })
                   .WithTags("Version")
                   .WithName("SaveNewVersion");
            }
        }
    }
}
