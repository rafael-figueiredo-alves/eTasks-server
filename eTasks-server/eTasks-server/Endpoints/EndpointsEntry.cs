namespace eTasks_server.Endpoints
{
    public static class EndpointsEntry
    {
        extension(WebApplication app)
        {
            public async Task AddAPIEndpoints()
            {
                var API_V2 = app.MapGroup("/api")
                                    .MapGroup("/v2");

                API_V2.MapVersionEndpoints();
                API_V2.MapUtilsEndpoints();
                API_V2.MapAuthEndpoints();
                API_V2.MapWebAuthEndpoints();
                API_V2.MapUserAdminEndpoints();
                API_V2.MapUsuariosEndpoints();
            }
        }
    }
}
