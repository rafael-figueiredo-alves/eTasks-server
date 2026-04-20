using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Entities.Version;
using Scalar.AspNetCore;

namespace eTasks_server.Endpoints.Version
{
    public static class VersionEndpoint
    {
        public static void MapVersionEndpoints(this IEndpointRouteBuilder app)
        {
            //Adiciona o endpoint para obter a versão atual do aplicativo
            GetVersionAsync(app);

            //Adiciona o endpoint para salvar as edições da versão
            ChangeVersionDetails(app);
        }

        private static void GetVersionAsync(IEndpointRouteBuilder app)
        {
            app.MapGet("/version", async (IVersionBLL versionBLL) =>
            {
                try
                {
                    return Results.Ok(await versionBLL.GetVersionAsync());
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Ocorreu um erro ao obter a versão: {ex.Message}");
                }
            })
               .WithTags("Versão da aplicação")
               .WithName("Obter versão do eTasks")
               .WithDescription("Retorna a versão atual do aplicativo, incluindo informações como número da versão, URL para download, etc.")
               .WithSummary("Obtém a versão atual do aplicativo")
               .WithDisplayName("Obter versão do eTasks")
               .Produces(StatusCodes.Status200OK, typeof(eTasksVersion))
               .Produces(StatusCodes.Status500InternalServerError);
        }

        private static void ChangeVersionDetails(IEndpointRouteBuilder app)
        {
            app.MapPut("/version", async (IVersionBLL versionBLL, eTasksVersion version) =>
            {
                if (await versionBLL.SaveNewVersionAsync(version))
                    return Results.Ok(version);
                else
                    return Results.BadRequest("Não foi possível salvar edições da versão");
            })
            .WithTags("Versão da aplicação")
            .WithName("Salvar alterações na versão do App")
            .WithDisplayName("Salvar alterações na versão do App")
            .WithSummary("Salva as alterações na versão do aplicativo")
            .RequireAuthorization("WebAdmin")
            .ExcludeFromDescription()
            .Produces(StatusCodes.Status200OK, typeof(eTasksVersion))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .WithDescription("Permite salvar as edições da versão do aplicativo. Requer autorização de administrador.");
        }
    }
}
