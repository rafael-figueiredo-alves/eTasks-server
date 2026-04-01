using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints
{
    public static class WebAuthEndpoints
    {
        public static IEndpointRouteBuilder MapWebAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/web-auth").WithTags("Autenticação Web");

            group.MapPost("/login", async (HttpContext context, [FromBody] WebLoginRequest request, IWebAuthBLL webAuthBLL) =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                await webAuthBLL.LoginAsync(context, request, ip);
                return Results.Ok();
            })
            .AllowAnonymous()
            .WithName("WebLogin");

            group.MapPost("/logout", async (HttpContext context, IWebAuthBLL webAuthBLL) =>
            {
                await webAuthBLL.LogoutAsync(context);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("WebLogout");

            return app;
        }
    }
}
