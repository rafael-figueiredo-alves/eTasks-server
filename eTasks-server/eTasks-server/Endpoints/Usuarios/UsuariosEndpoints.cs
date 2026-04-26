using System.Security.Claims;
using System.Text;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Usuarios
{
    public static class UsuariosEndpoints
    {
        public static IEndpointRouteBuilder MapUsuariosEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/usuarios")
                .WithTags("Usuarios")
                .RequireAuthorization();

            group.MapGet("/", async (ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
            {
                var userUid = user.GetRequiredUserUid();
                return Results.Ok(await userProfileBLL.GetProfileAsync(userUid));
            })
            .WithName("GetCurrentUserProfile")
            .WithSummary("Obtem o perfil do usuario autenticado.");

            group.MapPut("/", async (ClaimsPrincipal user, [FromBody] UpdateUserProfileRequest request, IUserProfileBLL userProfileBLL, CancellationToken cancellationToken) =>
            {
                var userUid = user.GetRequiredUserUid();
                return Results.Ok(await userProfileBLL.UpdateProfileAsync(userUid, request, cancellationToken));
            })
            .WithName("UpdateCurrentUserProfile")
            .WithSummary("Atualiza nome, e-mail e foto do usuario autenticado.");

            group.MapPatch("/", async (ClaimsPrincipal user, [FromBody] PatchUserSettingsRequest request, IUserProfileBLL userProfileBLL) =>
            {
                var userUid = user.GetRequiredUserUid();
                return Results.Ok(await userProfileBLL.PatchSettingsAsync(userUid, request));
            })
            .WithName("PatchCurrentUserSettings")
            .WithSummary("Atualiza parcialmente as configuracoes do usuario autenticado.");

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
            .WithSummary("Obtem as configuracoes do usuario autenticado com suporte a ETag.");

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
            .WithSummary("Atualiza as configuracoes do usuario autenticado com concorrencia otimista.");

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
            .WithSummary("Obtem pontos e conquistas do usuario autenticado com suporte a ETag.");

            group.MapPost("/sync", async (ClaimsPrincipal user, [FromBody] SyncUserDataRequest request, IUserProfileBLL userProfileBLL) =>
            {
                var userUid = user.GetRequiredUserUid();
                return Results.Ok(await userProfileBLL.SyncUserDataAsync(userUid, request));
            })
            .WithName("SyncCurrentUserData")
            .WithSummary("Retorna configuracoes e dados de gamificacao alterados desde o ultimo cursor do cliente.");

            group.MapPost("/exportar-csv", async (ClaimsPrincipal user, IUserProfileBLL userProfileBLL) =>
            {
                var userUid = user.GetRequiredUserUid();
                var csv = await userProfileBLL.ExportProfileCsvAsync(userUid);
                var fileName = $"usuario-{userUid:N}.csv";
                return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            })
            .WithName("ExportCurrentUserProfileCsv")
            .WithSummary("Baixa os dados do usuario autenticado em CSV.");

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
            .WithSummary("Remove logicamente a conta do usuario autenticado e revoga suas sessoes.");

            return app;
        }
    }
}
