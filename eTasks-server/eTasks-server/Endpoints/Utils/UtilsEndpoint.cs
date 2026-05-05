using eTasks_server.Models.Utils;

namespace eTasks_server.Endpoints.Utils
{
    public static class UtilsEndpoint
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia recursos de utiidades da api
            /// </summary>
            public void MapUtilsEndpoints()
            {
                app.GetOnlineState();
                app.GetServerTime();
                app.GetIP();
            }

            #region Métodos dos serviços disponíveis
            /// <summary>
            /// Saber se servidor está online, útil para monitoramento e health checks.
            /// </summary>
            private void GetOnlineState()
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

            /// <summary>
            /// Obter data e hora atual do servidor, útil para sincronização de tempo e monitoramento.
            /// </summary>
            private void GetServerTime()
            {
                app.MapGet("/server-time", () => Results.Ok(SaoPauloDateTime.Now()))
                    .AllowAnonymous()
                    .WithDescription("Endpoint para obter a hora atual do servidor.")
                    .WithDisplayName("Utilidades")
                    .WithName("Hora servidor")
                    .WithSummary("Obtém a hora atual do servidor.")
                    .WithTags("Utilidades")
                    .Produces<DateTime>(StatusCodes.Status200OK)
                    .WithDisplayName("Obter a hora atual do servidor");
            }

            /// <summary>
            /// Obter IP do cliente, útil para logs, monitoramento e segurança.
            /// </summary>
            private void GetIP()
            {
                app.MapGet("/ip", (HttpContext context) =>
                {
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Desconhecido";
                    return Results.Ok(new { IP = ipAddress });
                })
                .AllowAnonymous()
                .WithDescription("Endpoint para obter o endereço IP do cliente.")
                .WithDisplayName("Utilidades")
                .WithName("IP")
                .WithSummary("Obtém o endereço IP do usuário da requisição")
                .WithTags("Utilidades")
                .Produces(StatusCodes.Status200OK, typeof(object))
                .WithDisplayName("Obter endereço IP do cliente");
            }
            #endregion
        }
    }
}
