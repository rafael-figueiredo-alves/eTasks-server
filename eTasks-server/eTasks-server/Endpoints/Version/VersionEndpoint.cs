using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Entities.Version;
using eTasks_server.Models.Exceptions;
using Scalar.AspNetCore;

namespace eTasks_server.Endpoints.Version
{
    /// <summary>
    /// Classe responsável por definir os endpoints relacionados à versão do aplicativo eTasks.
    /// </summary>
    public static class VersionEndpoint
    {
        /// <summary>
        /// Mapea endpoints relacionados à versão do aplicativo eTasks, incluindo a obtenção da versão atual e a possibilidade de salvar edições na versão (requer autorização de administrador).
        /// </summary>
        /// <param name="app"></param>
        public static void MapVersionEndpoints(this IEndpointRouteBuilder app)
        {
            //Adiciona o endpoint para obter a versão atual do aplicativo
            GetVersionAsync(app);

            //Adiciona o endpoint para salvar as edições da versão
            ChangeVersionDetails(app);
        }

        #region Métodos de ações do endpoint de versões
        /// <summary>
        /// Obtem dados da versão atual do aplicativo eTasks, incluindo informações como número da versão, URL para download, etc. Retorna um objeto eTasksVersion contendo os detalhes da versão atual. Em caso de erro, retorna uma resposta de problema com a mensagem de erro. Requer autorização de administrador para acessar este endpoint.
        /// </summary>
        /// <param name="app"></param>
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

        /// <summary>
        /// Configura o endpoint para salvar alterações na versão do aplicativo, mapeando a rota HTTP PUT '/version'.
        /// </summary>
        /// <remarks>O endpoint exige autorização com a política 'WebAdmin' e retorna um objeto
        /// eTasksVersion em caso de sucesso ou um erro de validação em caso de falha. O endpoint é excluído da
        /// descrição pública da API.</remarks>
        /// <param name="app">O construtor de rotas de endpoint usado para registrar o endpoint na aplicação.</param>
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
        #endregion
    }
}
