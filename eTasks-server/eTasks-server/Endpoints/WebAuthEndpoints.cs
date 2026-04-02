using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints
{
    public static class WebAuthEndpoints
    {
        public static IEndpointRouteBuilder MapWebAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/web-auth").WithTags("Autenticação Web");

            group.MapPost("/login", async (HttpContext context, [FromForm] WebLoginRequest request, [FromQuery] string? returnUrl, IWebAuthBLL webAuthBLL) =>
            {
                try
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString();
                    await webAuthBLL.LoginAsync(context, request, ip);
                    return Results.LocalRedirect(GetSafeReturnUrl(returnUrl));
                }
                catch (ApiException ex)
                {
                    return Results.LocalRedirect(BuildLoginRedirect(returnUrl, ex.StatusCode == HttpStatusCode.Forbidden
                        ? "Acesso restrito. Apenas administradores podem acessar o painel."
                        : ex.Message));
                }
                catch
                {
                    return Results.LocalRedirect(BuildLoginRedirect(returnUrl, "Não foi possível realizar o login."));
                }
            })
            .AllowAnonymous()
            .ExcludeFromDescription()
            .WithName("WebLogin");

            group.MapGet("/logout", async (HttpContext context, [FromQuery] string? returnUrl, IWebAuthBLL webAuthBLL) =>
            {
                await webAuthBLL.LogoutAsync(context);
                return Results.LocalRedirect(GetSafeReturnUrl(returnUrl, "/login"));
            })
            .AllowAnonymous()
            .ExcludeFromDescription()
            .WithName("WebLogout");

            return app;
        }

        private static string BuildLoginRedirect(string? returnUrl, string errorMessage)
        {
            var safeReturnUrl = GetSafeReturnUrl(returnUrl);
            return $"/login?returnUrl={Uri.EscapeDataString(safeReturnUrl)}&error={Uri.EscapeDataString(errorMessage)}";
        }

        private static string GetSafeReturnUrl(string? returnUrl, string fallback = "/")
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            {
                return fallback;
            }

            return returnUrl;
        }
    }
}
