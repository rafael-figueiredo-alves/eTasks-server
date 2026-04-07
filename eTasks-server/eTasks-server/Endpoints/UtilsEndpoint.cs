namespace eTasks_server.Endpoints
{
    public static class UtilsEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            public void MapUtilsEndpoints()
            {
                app.MapGet("/online", () => Results.NoContent())
                    .AllowAnonymous()
                    .WithDescription("Endpoint para verificar se o servidor está online.")
                    .WithDisplayName("Utilidades")
                    .WithName("Utilidades")
                    .WithSummary("Verifica se o servidor está online.")
                    .WithTags("Utilidades")
                    .Produces(StatusCodes.Status204NoContent)
                    .WithDisplayName("Verificar se o servidor está online");
            }
        }
    }
}
