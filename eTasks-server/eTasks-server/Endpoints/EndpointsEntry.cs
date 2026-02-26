namespace eTasks_server.Endpoints
{
    public static class EndpointsEntry
    {
        extension(WebApplication app)
        {
            public async Task AddEndpoints()
            {
                var API_V2 = app.MapGroup("/api")
                                    .MapGroup("/v2");

                await API_V2.MapVersionEndpoints();
                API_V2.MapUtilsEndpoints();
                API_V2.MapDirectoriesEndpoint(app.Environment);
                API_V2.MapUpDirectoriesEndpoint(app.Environment);

            }
        }
    }
}
