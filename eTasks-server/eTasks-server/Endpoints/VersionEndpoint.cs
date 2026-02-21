using eTasks_server.Core.BusinessLayers;
using eTasks_server.Core.Data;

namespace eTasks_server.Endpoints
{
    public static class VersionEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public void MapVersionEndpoints()
            {
                app.MapGet("/version", async (AppDbContext dbContext) => Results.Ok(await VersionBLL.GetVersion(dbContext)))
                   .WithTags("Version")
                   .WithName("GetVersion");
            }
        }
    }
}
