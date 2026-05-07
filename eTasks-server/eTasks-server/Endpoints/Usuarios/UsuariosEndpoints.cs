using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace eTasks_server.Endpoints.Usuarios
{
    public static class UsuariosEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            public IEndpointRouteBuilder MapUsuariosEndpoints()
            {
                var group = app.MapGroup("/usuarios")
                    .WithTags("Usuários")
                    .RequireAuthorization();

                group
                    .GetUserProfile()
                    .UpdateUserProfile()
                    .UpdateUserSettings()
                    .GetUserSettings()
                    .UpdateUserSettingsWithConcurrency()
                    .GetUserBonusInfo()
                    .GetUserSync()
                    .ExportUserAccountToCSV()
                    .DeleteUserAccount();

                return app;
            }
        }

        /// <summary>
        /// Métodos de extensão para configurar os endpoints relacionados ao perfil do usuário autenticado, incluindo obtenção e atualização de perfil, configurações, informações de bônus, sincronização de dados, exportação de conta e exclusão de conta.
        /// </summary>
        /// <param name="group"></param>
        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Obtém o perfil do usuário autenticado, incluindo nome, e-mail, foto e outras informações básicas.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder GetUserProfile()
            {
                group.MapGet("/", async (ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();
                    
                    return Results.Ok(await userProfileBLL.GetProfileAsync(userUid));
                })
                .WithName("GetCurrentUserProfile")
                .WithSummary("Obtém o perfil do usuário autenticado.")
                .WithDescription("Retorna as informações básicas do perfil do usuário atualmente autenticado, como nome, e-mail, foto e outras configurações relacionadas ao perfil.")
                .Produces(StatusCodes.Status200OK, typeof(UserProfileResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);                

                return group;
            }

            /// <summary>
            /// Atualiza o perfil do usuário autenticado, permitindo alterar nome, e-mail e foto. O campo PhotoBase64 deve conter a nova foto em base64 ou ser nulo para manter a foto atual. Se RemovePhoto for true, a foto será removida independentemente do valor de PhotoBase64. O endpoint retorna o perfil atualizado após a modificação.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder UpdateUserProfile()
            {
                group.MapPut("/", async (ClaimsPrincipal user, [FromBody] UpdateUserProfileRequest request, IUserProfileBLL userProfileBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    return Results.Ok(await userProfileBLL.UpdateProfileAsync(userUid, request, cancellationToken));
                })
                .WithName("UpdateCurrentUserProfile")
                .WithSummary("Atualiza nome, e-mail e foto do usuário autenticado.")
                .WithDescription("Permite alterar o nome, e-mail e foto do usuário atualmente autenticado. O campo PhotoBase64 deve conter a nova foto em base64 ou ser nulo para manter a foto atual. Se RemovePhoto for true, a foto será removida independentemente do valor de PhotoBase64. O endpoint retorna o perfil atualizado após a modificação.")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status200OK, typeof(UserProfileResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Atualiza parcialmente as configurações do usuário autenticado, como tema, idioma, tela inicial e preferências do sistema de bônus. O endpoint aceita apenas os campos que devem ser alterados, mantendo os demais inalterados. Retorna as configurações atualizadas após a modificação.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder UpdateUserSettings()
            {
                group.MapPatch("/", async (ClaimsPrincipal user, [FromBody] PatchUserSettingsRequest request, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    return Results.Ok(await userProfileBLL.PatchSettingsAsync(userUid, request));
                })
                .WithName("PatchCurrentUserSettings")
                .WithSummary("Atualiza parcialmente as configurações do usuário autenticado.")
                .WithDescription("Permite atualizar parcialmente as configurações do usuário atualmente autenticado, como tema, idioma, tela inicial e preferências do sistema de bônus. O endpoint aceita apenas os campos que devem ser alterados, mantendo os demais inalterados. Retorna as configurações atualizadas após a modificação.")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status200OK, typeof(UserSettingsDTO))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Configura o endpoint para obter as configurações do usuário autenticado com suporte a ETag.
            /// </summary>
            /// <remarks>O endpoint mapeado utiliza ETag para otimizar o tráfego, retornando o status
            /// 304 Not Modified quando apropriado. As configurações retornadas correspondem ao usuário atualmente
            /// autenticado no contexto da requisição.</remarks>
            /// <returns>Um objeto <see cref="RouteGroupBuilder"/> configurado com o endpoint de obtenção das configurações do
            /// usuário autenticado.</returns>
            public RouteGroupBuilder GetUserSettings()
            {
                group.MapGet("/settings", async (HttpContext context, ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    var settings = await userProfileBLL.GetSettingsAsync(userUid);

                    var etag = UserProfileEtagHelper.BuildSettingsEtag(settings);

                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(context.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    context.Response.Headers.ETag = etag;
                    return Results.Ok(settings);
                })
                .WithName("GetCurrentUserSettings")
                .WithSummary("Obtém as configurações do usuário autenticado com suporte a ETag.")
                .WithDescription("Retorna as configurações do usuário atualmente autenticado, como tema, idioma, tela inicial e preferências do sistema de bônus. O endpoint utiliza ETag para otimizar o tráfego, retornando o status 304 Not Modified quando apropriado.")
                .Produces(StatusCodes.Status200OK, typeof(UserSettingsSyncResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }
        
            /// <summary>
            /// Configura o endpoint PATCH para atualizar as configurações do usuário autenticado utilizando controle de
            /// concorrência otimista.
            /// </summary>
            /// <remarks>O endpoint utiliza o cabeçalho HTTP If-Match para garantir que as
            /// configurações do usuário não foram modificadas por outro cliente desde a última leitura. Se o ETag não
            /// corresponder, a solicitação será rejeitada, orientando o cliente a atualizar os dados antes de tentar
            /// novamente.</remarks>
            /// <returns>O objeto <see cref="RouteGroupBuilder"/> configurado com o endpoint de atualização de configurações do
            /// usuário.</returns>
            public RouteGroupBuilder UpdateUserSettingsWithConcurrency()
            {
                group.MapPatch("/settings", async (HttpContext context, ClaimsPrincipal user, [FromBody] PatchUserSettingsRequest request, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();
                    var currentSettings = await userProfileBLL.GetSettingsAsync(userUid);
                    ApiResourceHttpHelper.EnsureIfMatch(context.Request, UserProfileEtagHelper.BuildSettingsEtag(currentSettings), "As configuracoes do usuario foram alteradas por outro cliente. Atualize os dados e tente novamente.");

                    await userProfileBLL.PatchSettingsAsync(userUid, request);
                    var updatedSettings = await userProfileBLL.GetSettingsAsync(userUid);
                    context.Response.Headers.ETag = UserProfileEtagHelper.BuildSettingsEtag(updatedSettings);
                    return Results.Ok(updatedSettings);
                })
                .WithName("PatchCurrentUserSettingsOfflineFirst")
                .WithSummary("Atualiza as configuracoes do usuario autenticado com concorrencia otimista.")
                .WithDescription("Permite atualizar as configurações do usuário autenticado utilizando controle de concorrência otimista. O endpoint utiliza o cabeçalho HTTP If-Match para garantir que as configurações do usuário não foram modificadas por outro cliente desde a última leitura. Se o ETag não corresponder, a solicitação será rejeitada, orientando o cliente a atualizar os dados antes de tentar novamente.")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status200OK, typeof(UserSettingsSyncResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status412PreconditionFailed)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }
        
            /// <summary>
            /// Configura o endpoint de API que obtém as informações de pontos e conquistas do usuário autenticado, com
            /// suporte a validação de ETag para otimização de cache.
            /// </summary>
            /// <remarks>O endpoint responde com status 200 e os dados de bônus do usuário, ou com
            /// status 304 se o ETag enviado pelo cliente corresponder ao estado atual dos dados. Requer autenticação do
            /// usuário.</remarks>
            /// <returns>O objeto <see cref="RouteGroupBuilder"/> configurado com o endpoint para recuperar as informações de
            /// bônus do usuário autenticado.</returns>
            public RouteGroupBuilder GetUserBonusInfo()
            {
                group.MapGet("/bonus", async (HttpContext context, ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    var bonus = await userProfileBLL.GetBonusAsync(userUid);

                    var etag = UserProfileEtagHelper.BuildBonusEtag(bonus);

                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(context.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    context.Response.Headers.ETag = etag;
                    return Results.Ok(bonus);
                })
                .WithName("GetCurrentUserBonus")
                .WithSummary("Obtém pontos e conquistas do usuário autenticado com suporte a ETag.")
                .WithDescription("Retorna as informações de pontos e conquistas do usuário atualmente autenticado. O endpoint utiliza ETag para otimizar o tráfego, retornando o status 304 Not Modified quando apropriado.")
                .Produces(StatusCodes.Status200OK, typeof(UserBonusSyncResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }
        
            /// <summary>
            /// Adiciona um endpoint que permite sincronizar as configurações e dados de gamificação do usuário
            /// autenticado, retornando apenas as informações alteradas desde o último cursor fornecido pelo cliente.
            /// </summary>
            /// <remarks>O endpoint POST '/sync' aceita um cursor opaco que representa o estado dos
            /// dados na última sincronização, permitindo que apenas alterações incrementais sejam retornadas. Requer
            /// autenticação do usuário. Retorna um objeto contendo as configurações e dados de gamificação modificados
            /// desde o último cursor informado.</remarks>
            /// <returns>O construtor de grupo de rotas atualizado com o endpoint de sincronização de dados do usuário.</returns>
            public RouteGroupBuilder GetUserSync()
            {
                group.MapPost("/sync", async (ClaimsPrincipal user, [FromBody] SyncUserDataRequest request, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    return Results.Ok(await userProfileBLL.SyncUserDataAsync(userUid, request));
                })
                .WithName("SyncCurrentUserData")
                .WithSummary("Retorna configurações e dados de gamificação alterados desde o último cursor do cliente.")
                .WithDescription("Permite que o cliente sincronize as configurações e dados de gamificação do usuário autenticado, retornando apenas as informações que foram alteradas desde o último cursor fornecido pelo cliente. O cursor é um valor opaco que representa o estado dos dados no momento da última sincronização, permitindo uma atualização eficiente e incremental.")
                .Produces(StatusCodes.Status200OK, typeof(UserDataSyncResponse))
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }
        
            /// <summary>
            /// Configura um endpoint que permite ao usuário autenticado exportar seus dados de perfil em formato CSV
            /// por meio de uma solicitação HTTP POST.
            /// </summary>
            /// <remarks>O endpoint '/exportar-csv' retorna um arquivo CSV contendo os dados do perfil
            /// do usuário autenticado. O arquivo é gerado dinamicamente e nomeado com o identificador do usuário.
            /// Apenas usuários autenticados podem acessar este recurso.</remarks>
            /// <returns>O objeto <see cref="RouteGroupBuilder"/> configurado com o endpoint de exportação de perfil do usuário
            /// em CSV.</returns>
            public RouteGroupBuilder ExportUserAccountToCSV()
            {
                group.MapPost("/exportar-csv", async (ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    var csv = await userProfileBLL.ExportProfileCsvAsync(userUid);

                    var fileName = $"usuario-{userUid:N}.csv";

                    return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
                })
                .WithName("ExportCurrentUserProfileCsv")
                .WithSummary("Baixa os dados do usuário autenticado em CSV.")
                .WithDescription("Permite que o usuário autenticado exporte seus dados de perfil em formato CSV por meio de uma solicitação HTTP POST. O endpoint retorna um arquivo CSV contendo os dados do perfil do usuário autenticado, gerado dinamicamente e nomeado com o identificador do usuário. Apenas usuários autenticados podem acessar este recurso.")
                .Produces(StatusCodes.Status200OK, contentType: "text/csv")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }
        
            /// <summary>
            /// Configura um endpoint que remove logicamente a conta do usuário autenticado e revoga suas sessões.
            /// </summary>
            /// <remarks>O endpoint realiza a exclusão lógica da conta do usuário atualmente
            /// autenticado e remove o cookie de refresh token associado. A operação é protegida e requer autenticação.
            /// Após a exclusão, o usuário não poderá mais acessar recursos protegidos até que uma nova conta seja
            /// criada.</remarks>
            /// <returns>O objeto <see cref="RouteGroupBuilder"/> atualizado com o endpoint de exclusão de conta configurado.</returns>
            public RouteGroupBuilder DeleteUserAccount()
            {
                group.MapDelete("/", async (HttpContext context, ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();
                    await userProfileBLL.SoftDeleteAsync(userUid);
                    context.Response.Cookies.Delete(Constants.RefreshTokenCookieName, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Path = "/api"
                    });

                    return Results.Ok(new { Message = "Conta removida com sucesso." });
                })
                .WithName("DeleteCurrentUserAccount")
                .WithSummary("Remove logicamente a conta do usuário autenticado e revoga suas sessões.")
                .WithDescription("Permite que o usuário autenticado remova logicamente sua conta, tornando-a inativa e inacessível para futuras autenticações. O endpoint também revoga as sessões ativas do usuário, garantindo que ele seja desconectado de todos os dispositivos. A operação é protegida e requer autenticação. Após a exclusão, o usuário não poderá mais acessar recursos protegidos até que uma nova conta seja criada.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }
        }
    }
}
