using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Goals
{
    public static class GoalsEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de metas sob a rota base "/goals". Todas as operacoes exigem JWT Bearer,
            /// usam o usuario autenticado como escopo de dados e preservam os contratos offline-first com ETag,
            /// If-None-Match, If-Match, sync incremental e push sync.
            /// </summary>
            /// <returns>O proprio <see cref="IEndpointRouteBuilder"/> para encadeamento do bootstrap.</returns>
            public IEndpointRouteBuilder MapGoalsEndpoints()
            {
                var group = app.MapGroup("/goals")
                    .WithTags("Metas")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.ListGoalsEndpoint()
                     .PushSyncGoalsEndpoint()
                     .SyncGoalsEndpoint()
                     .GetGoalEndpoint()
                     .CreateGoalEndpoint()
                     .UpdateGoalEndpoint()
                     .DeleteGoalEndpoint();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Lista as metas do usuario autenticado. Retorna ETag da colecao filtrada e responde 304
            /// quando o cliente envia If-None-Match compativel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder ListGoalsEndpoint()
            {
                group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListGoalsRequest request, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var goals = await goalBLL.ListAsync(userUid, request, cancellationToken);
                    var etag = GoalEtagHelper.BuildListEtag(goals, request);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(goals);
                })
                .WithName("ListGoals")
                .WithSummary("Lista as metas do usuario autenticado.")
                .WithDescription("Retorna apenas metas do usuario autenticado, com filtros por status, tipo, prioridade, recompensa configurada e termo de busca. O endpoint retorna ETag e aceita If-None-Match para cache.")
                .Produces(StatusCodes.Status200OK, typeof(List<GoalListItemResponse>))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Processa a outbox offline de metas. Cada mutacao e tratada de forma independente para que
            /// falhas parciais nao bloqueiem o restante do lote.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder PushSyncGoalsEndpoint()
            {
                group.MapPost("/push-sync", async (HttpContext httpContext, [FromBody] GoalPushSyncRequest request, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = new GoalPushSyncResponse
                    {
                        ServerTime = SaoPauloDateTime.Now()
                    };

                    foreach (var operation in request.Operations ?? [])
                    {
                        response.Results.Add(await ProcessPushOperationAsync(userUid, operation, goalBLL, cancellationToken));
                    }

                    return Results.Ok(response);
                })
                .WithName("PushSyncGoals")
                .WithSummary("Processa em lote as mutacoes pendentes da outbox de metas.")
                .WithDescription("Recebe operacoes de create, update e delete geradas offline pelo cliente. Cada item e processado individualmente com retorno de sucesso, conflito, validacao ou falha.")
                .Produces(StatusCodes.Status200OK, typeof(GoalPushSyncResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Retorna alteracoes incrementais desde o cursor informado pelo cliente, mantendo o contrato
            /// offline-first de upserts e tombstones.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SyncGoalsEndpoint()
            {
                group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncGoalsRequest request, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var payload = await goalBLL.SyncAsync(userUid, request, cancellationToken);
                    return Results.Ok(payload);
                })
                .WithName("SyncGoals")
                .WithSummary("Retorna alteracoes incrementais de metas para sincronizacao offline-first.")
                .WithDescription("Usa o cursor Since para retornar upserts e tombstones desde a ultima sincronizacao do cliente.")
                .Produces(StatusCodes.Status200OK, typeof(GoalSyncResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Obtem uma meta especifica do usuario autenticado. Retorna ETag do recurso e aceita If-None-Match.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetGoalEndpoint()
            {
                group.MapGet("/{goalId:guid}", async (HttpContext httpContext, Guid goalId, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var goal = await goalBLL.GetByIdAsync(userUid, goalId, cancellationToken);
                    var etag = GoalEtagHelper.BuildDetailsEtag(goal);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(goal);
                })
                .WithName("GetGoal")
                .WithSummary("Obtem os detalhes completos de uma meta.")
                .WithDescription("Retorna apenas a meta pertencente ao usuario autenticado. O endpoint retorna ETag e aceita If-None-Match para cache.")
                .Produces(StatusCodes.Status200OK, typeof(GoalDetailsResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Cria uma meta para o usuario autenticado e devolve a representacao criada com ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder CreateGoalEndpoint()
            {
                group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateGoalRequest request, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var goal = await goalBLL.CreateAsync(userUid, request, cancellationToken);
                    httpContext.Response.Headers.ETag = GoalEtagHelper.BuildDetailsEtag(goal);
                    return Results.Created($"/api/v2/goals/{goal.Id}", goal);
                })
                .WithName("CreateGoal")
                .WithSummary("Cria uma nova meta para o usuario autenticado.")
                .WithDescription("Permite cadastrar metas com tipo, prioridade, status e pontos de recompensa fixos opcionais. Se a meta ja nascer concluida, a recompensa configurada ou a regra GoalCompletion ativa pode ser aplicada.")
                .Produces(StatusCodes.Status201Created, typeof(GoalDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Atualiza uma meta do usuario autenticado, respeitando If-Match para concorrencia otimista.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateGoalEndpoint()
            {
                group.MapPut("/{goalId:guid}", async (HttpContext httpContext, Guid goalId, [FromBody] UpdateGoalRequest request, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentGoal = await goalBLL.GetByIdAsync(userUid, goalId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, GoalEtagHelper.BuildDetailsEtag(currentGoal), "A meta foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    var updatedGoal = await goalBLL.UpdateAsync(userUid, goalId, request, cancellationToken);
                    httpContext.Response.Headers.ETag = GoalEtagHelper.BuildDetailsEtag(updatedGoal);
                    return Results.Ok(updatedGoal);
                })
                .WithName("UpdateGoal")
                .WithSummary("Edita uma meta do usuario autenticado.")
                .WithDescription("Aceita If-Match para concorrencia otimista via ETag. Se o status mudar para concluida, a recompensa da meta e ajustada automaticamente.")
                .Produces(StatusCodes.Status200OK, typeof(GoalDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Remove logicamente uma meta do usuario autenticado, mantendo tombstone para sync.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder DeleteGoalEndpoint()
            {
                group.MapDelete("/{goalId:guid}", async (HttpContext httpContext, Guid goalId, IGoalBLL goalBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentGoal = await goalBLL.GetByIdAsync(userUid, goalId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, GoalEtagHelper.BuildDetailsEtag(currentGoal), "A meta foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    await goalBLL.DeleteAsync(userUid, goalId, cancellationToken);
                    return Results.NoContent();
                })
                .WithName("DeleteGoal")
                .WithSummary("Remove logicamente uma meta do usuario autenticado.")
                .WithDescription("Aceita If-Match para concorrencia otimista via ETag. A meta nao e apagada fisicamente: ela vira um tombstone para sincronizacao offline-first.")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            #region Metodos de apoio para push sync offline-first
            private static async Task<GoalPushSyncItemResponse> ProcessPushOperationAsync(Guid userUid, GoalPushSyncItemRequest operation, IGoalBLL goalBLL, CancellationToken cancellationToken)
            {
                try
                {
                    ValidatePushOperation(operation);

                    return operation.Operation switch
                    {
                        GoalPushOperationType.Create => await ApplyCreateAsync(userUid, operation, goalBLL, cancellationToken),
                        GoalPushOperationType.Update => await ApplyUpdateAsync(userUid, operation, goalBLL, cancellationToken),
                        GoalPushOperationType.Delete => await ApplyDeleteAsync(userUid, operation, goalBLL, cancellationToken),
                        _ => BuildFailure(operation, GoalPushSyncItemStatus.ValidationError, "invalid_operation", "Operacao de push sync invalida.")
                    };
                }
                catch (ValidationException ex)
                {
                    return BuildFailure(operation, GoalPushSyncItemStatus.ValidationError, "validation_error", ApiResourceHttpHelper.FlattenValidationErrors(ex));
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    return BuildFailure(operation, GoalPushSyncItemStatus.Conflict, "conflict", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (operation.Operation == GoalPushOperationType.Delete)
                    {
                        return new GoalPushSyncItemResponse
                        {
                            ClientMutationId = operation.ClientMutationId,
                            Status = GoalPushSyncItemStatus.Applied
                        };
                    }

                    return BuildFailure(operation, GoalPushSyncItemStatus.NotFound, "not_found", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex)
                {
                    return BuildFailure(operation, GoalPushSyncItemStatus.Failed, "api_error", ex.UserMessage ?? ex.Message);
                }
                catch (Exception ex)
                {
                    return BuildFailure(operation, GoalPushSyncItemStatus.Failed, "unexpected_error", ex.Message);
                }
            }

            private static async Task<GoalPushSyncItemResponse> ApplyCreateAsync(Guid userUid, GoalPushSyncItemRequest operation, IGoalBLL goalBLL, CancellationToken cancellationToken)
            {
                var goal = await goalBLL.CreateAsync(userUid, operation.Create!, cancellationToken);
                return new GoalPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = GoalPushSyncItemStatus.Applied,
                    Goal = goal,
                    ServerEtag = GoalEtagHelper.BuildDetailsEtag(goal)
                };
            }

            private static async Task<GoalPushSyncItemResponse> ApplyUpdateAsync(Guid userUid, GoalPushSyncItemRequest operation, IGoalBLL goalBLL, CancellationToken cancellationToken)
            {
                var currentGoal = await goalBLL.GetByIdAsync(userUid, operation.GoalId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentGoal);

                var goal = await goalBLL.UpdateAsync(userUid, operation.GoalId.Value, operation.Update!, cancellationToken);
                return new GoalPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = GoalPushSyncItemStatus.Applied,
                    Goal = goal,
                    ServerEtag = GoalEtagHelper.BuildDetailsEtag(goal)
                };
            }

            private static async Task<GoalPushSyncItemResponse> ApplyDeleteAsync(Guid userUid, GoalPushSyncItemRequest operation, IGoalBLL goalBLL, CancellationToken cancellationToken)
            {
                var currentGoal = await goalBLL.GetByIdAsync(userUid, operation.GoalId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentGoal);

                await goalBLL.DeleteAsync(userUid, operation.GoalId.Value, cancellationToken);
                return new GoalPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = GoalPushSyncItemStatus.Applied,
                    Deleted = new DeletedGoalResponse
                    {
                        Id = currentGoal.Id,
                        DeletedAt = SaoPauloDateTime.Now()
                    }
                };
            }

            private static void ValidatePushOperation(GoalPushSyncItemRequest operation)
            {
                if (string.IsNullOrWhiteSpace(operation.ClientMutationId))
                {
                    throw new ValidationException("ClientMutationId", "O identificador da mutacao do cliente e obrigatorio.");
                }

                switch (operation.Operation)
                {
                    case GoalPushOperationType.Create when operation.Create is null:
                        throw new ValidationException("Create", "O payload de criacao e obrigatorio.");
                    case GoalPushOperationType.Update when operation.GoalId is null || operation.Update is null:
                        throw new ValidationException("Update", "GoalId e payload de atualizacao sao obrigatorios.");
                    case GoalPushOperationType.Delete when operation.GoalId is null:
                        throw new ValidationException("GoalId", "GoalId e obrigatorio para exclusao.");
                }
            }

            private static void EnsureExpectedEtag(string? expectedEtag, GoalDetailsResponse currentGoal)
            {
                if (string.IsNullOrWhiteSpace(expectedEtag))
                {
                    return;
                }

                var currentEtag = GoalEtagHelper.BuildDetailsEtag(currentGoal);
                if (!string.Equals(expectedEtag.Trim(), currentEtag, StringComparison.Ordinal) && expectedEtag.Trim() != "*")
                {
                    throw new ApiException(HttpStatusCode.PreconditionFailed, "A meta foi alterada por outro cliente. Atualize os dados e tente novamente.");
                }
            }

            private static GoalPushSyncItemResponse BuildFailure(GoalPushSyncItemRequest operation, GoalPushSyncItemStatus status, string errorCode, string errorMessage)
            {
                return new GoalPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = status,
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage
                };
            }
            #endregion
        }
    }
}
