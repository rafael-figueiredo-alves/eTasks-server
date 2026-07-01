using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.DTOs.AI.Responses;
using eTasks_server.Models.Enums.Ai;
using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.AI
{
    public static class AiAssistantEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados ao assistente de IA, incluindo obtenção de capacidades e execução de assistências contextualizadas para usuários autenticados. Todos os endpoints exigem autenticação via JWT e estão agrupados sob a rota "/ai" com a tag "AI" para organização na documentação Swagger.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapAiAssistantEndpoints()
            {
                var group = app.MapGroup("/ai")
                    .WithTags("AI")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.Capabilities()
                     .Assist()
                     .AssistForResource();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Endpoint para obter as capacidades de IA disponíveis, listando os usos recomendados de IA por recurso dentro do eTasks. Retorna um objeto AiCapabilitiesResponse contendo as informações sobre as capacidades de IA, incluindo o modo do provedor, orientações transversais e recursos disponíveis. Requer autenticação e retorna status 200 OK com os dados ou 401 Unauthorized se o usuário não estiver autenticado.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder Capabilities()
            {
                group.MapGet("/capabilities", (IAiCapabilityCatalog capabilityCatalog) =>
                {
                    return Results.Ok(capabilityCatalog.GetCapabilities());
                })
                .WithName("GetAICapabilities")
                .WithSummary("Lista os usos recomendados de IA por recurso dentro do eTasks.")
                .Produces(StatusCodes.Status200OK, typeof(AiCapabilitiesResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Endpoint para executar uma assistência de IA contextualizada para o usuário autenticado. O cliente envia um AiAssistRequest contendo o recurso de IA, a intenção da interação, o prompt do usuário e contexto adicional opcional. O endpoint processa a solicitação usando o serviço IA e retorna um AiAssistResponse com a resposta gerada pela IA. Requer autenticação e retorna status 200 OK com a resposta ou erros apropriados para solicitações inválidas, falta de autenticação ou indisponibilidade do serviço de IA.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder Assist()
            {
                group.MapPost("/assist", async (HttpContext httpContext, [FromBody] AiAssistRequest request, IAiAssistantService aiAssistantService, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = await aiAssistantService.AssistAsync(userUid, request, cancellationToken);
                    return Results.Ok(response);
                })
                .WithName("AssistWithAI")
                .WithSummary("Executa uma assistência de IA contextualizada para o usuário autenticado.")
                .Produces(StatusCodes.Status200OK, typeof(AiAssistResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status503ServiceUnavailable, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Endpoint para executar uma assistência de IA específica para um recurso do eTasks. O cliente envia um AiAssistRequest contendo a intenção da interação, o prompt do usuário e contexto adicional opcional, direcionado a um recurso específico (como tarefas, metas, notas, etc.). O endpoint valida o recurso especificado na rota, processa a solicitação usando o serviço IA e retorna um AiAssistResponse com a resposta gerada pela IA. Requer autenticação e retorna status 200 OK com a resposta ou erros apropriados para solicitações inválidas, falta de autenticação ou indisponibilidade do serviço de IA.
            /// </summary>
            /// <returns></returns>
            /// <exception cref="ValidationException"></exception>
            private RouteGroupBuilder AssistForResource()
            {
                group.MapPost("/{resource}/assist", async (HttpContext httpContext, string resource, [FromBody] AiAssistRequest request, IAiAssistantService aiAssistantService, CancellationToken cancellationToken) =>
                {
                    if (!Enum.TryParse<AiResourceType>(resource, true, out var parsedResource))
                    {
                        throw new ValidationException("resource", "Recurso de IA inválido.");
                    }

                    request.Resource = parsedResource;
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = await aiAssistantService.AssistAsync(userUid, request, cancellationToken);
                    return Results.Ok(response);
                })
                .WithName("AssistWithAIForResource")
                .WithSummary("Executa assistência de IA para um recurso específico do usuário autenticado.")
                .Produces(StatusCodes.Status200OK, typeof(AiAssistResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status503ServiceUnavailable, typeof(ErrorResponse));

                return group;
            }
        }
    }
}
