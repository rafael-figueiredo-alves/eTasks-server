using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.DTOs.AI.Responses;
using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.AI
{
    public static class AiAssistantEndpoints
    {
        public static IEndpointRouteBuilder MapAiAssistantEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/ai")
                .WithTags("AI")
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });

            group.MapGet("/capabilities", (IAiCapabilityCatalog capabilityCatalog) =>
            {
                return Results.Ok(capabilityCatalog.GetCapabilities());
            })
            .WithName("GetAICapabilities")
            .WithSummary("Lista os usos recomendados de IA por recurso dentro do eTasks.")
            .Produces(StatusCodes.Status200OK, typeof(AiCapabilitiesResponse))
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/assist", async (HttpContext httpContext, [FromBody] AiAssistRequest request, IAiAssistantService aiAssistantService, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var response = await aiAssistantService.AssistAsync(userUid, request, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("AssistWithAI")
            .WithSummary("Executa uma assistencia de IA contextualizada para o usuario autenticado.")
            .Produces(StatusCodes.Status200OK, typeof(AiAssistResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable, typeof(ErrorResponse));

            group.MapPost("/{resource}/assist", async (HttpContext httpContext, string resource, [FromBody] AiAssistRequest request, IAiAssistantService aiAssistantService, CancellationToken cancellationToken) =>
            {
                if (!Enum.TryParse<AiResourceType>(resource, true, out var parsedResource))
                {
                    throw new ValidationException("resource", "Recurso de IA invalido.");
                }

                request.Resource = parsedResource;
                var userUid = httpContext.User.GetRequiredUserUid();
                var response = await aiAssistantService.AssistAsync(userUid, request, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("AssistWithAIForResource")
            .WithSummary("Executa assistencia de IA para um recurso especifico do usuario autenticado.")
            .Produces(StatusCodes.Status200OK, typeof(AiAssistResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status503ServiceUnavailable, typeof(ErrorResponse));

            return app;
        }
    }
}
