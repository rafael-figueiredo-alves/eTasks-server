namespace eTasks_server.Endpoints
{
    public static class VersionEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public void MapVersionEndpoint()
            {
                app.MapGet("/version", () => Results.Ok("eTasks-server version 2.0.0-beta-1"))
                   .WithTags("Version")
                   .WithName("GetVersion");
            }
        }
    }
}
