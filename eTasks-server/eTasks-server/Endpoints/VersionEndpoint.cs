using eTasks_server.Core.BusinessLayers;

namespace eTasks_server.Endpoints
{
    public static class VersionEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public void MapVersionEndpoint()
            {
                app.MapGet("/version", () => Results.Ok(VersionBLL.GetVersion()))
                   .WithTags("Version")
                   .WithName("GetVersion");
            }
        }
    }
}
