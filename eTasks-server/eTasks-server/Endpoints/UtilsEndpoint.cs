namespace eTasks_server.Endpoints
{
    public static class UtilsEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public void MapUtilsEndpoints()
            {
                app.MapGet("/checkconnection", () => Results.NoContent());
            }

            public void MapDirectoriesEndpoint(IWebHostEnvironment env)
            {
                app.MapGet("/directories", () => env.ContentRootPath);
            }

            public void MapUpDirectoriesEndpoint(IWebHostEnvironment env)
            {
                app.MapGet("/updirectories", () => Directory.GetParent(env.ContentRootPath)?.FullName);
            }
        }
    }
}
