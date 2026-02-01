namespace eTasks_server.Endpoints
{
    public static class EndpointsEntry
    {
        extension(WebApplication app)
        {
            public void AddEndpoints()
            {
                app.MapGroup("/api")
                   .MapGroup("/v2")
                    .MapVersionEndpoint();
            }
        }
    }
}
