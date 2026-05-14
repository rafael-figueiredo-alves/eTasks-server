using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Shopping.Requests;
using eTasks_server.Models.DTOs.Shopping.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Shopping
{
    public static class ShoppingListsEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de listas de compras sob a rota base "/shopping". Todas as operacoes exigem
            /// JWT Bearer, usam o usuario autenticado como escopo de dados e preservam ETag, If-None-Match,
            /// If-Match, sync incremental e push sync.
            /// </summary>
            /// <returns>O proprio <see cref="IEndpointRouteBuilder"/> para encadeamento do bootstrap.</returns>
            public IEndpointRouteBuilder MapShoppingListsEndpoints()
            {
                var group = app.MapGroup("/shopping")
                    .WithTags("Compras")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.ListShoppingListsEndpoint()
                     .PushSyncShoppingListsEndpoint()
                     .SyncShoppingListsEndpoint()
                     .GetShoppingListEndpoint()
                     .CreateShoppingListEndpoint()
                     .UpdateShoppingListEndpoint()
                     .DeleteShoppingListEndpoint();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Lista as listas de compras do usuario autenticado. Retorna ETag da colecao filtrada e responde
            /// 304 quando o cliente envia If-None-Match compativel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder ListShoppingListsEndpoint()
            {
                group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListShoppingListsRequest request, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var lists = await shoppingListBLL.ListAsync(userUid, request, cancellationToken);
                    var etag = ShoppingListEtagHelper.BuildListEtag(lists, request);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(lists);
                })
                .Produces(StatusCodes.Status200OK, typeof(List<ShoppingListListItemResponse>));

                return group;
            }

            /// <summary>
            /// Processa a outbox offline de compras. Cada mutacao e tratada de forma independente para que
            /// falhas parciais nao bloqueiem o restante do lote.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder PushSyncShoppingListsEndpoint()
            {
                group.MapPost("/push-sync", async (HttpContext httpContext, [FromBody] ShoppingListPushSyncRequest request, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = new ShoppingListPushSyncResponse { ServerTime = SaoPauloDateTime.Now() };

                    foreach (var operation in request.Operations ?? [])
                    {
                        response.Results.Add(await ProcessPushOperationAsync(userUid, operation, shoppingListBLL, cancellationToken));
                    }

                    return Results.Ok(response);
                })
                .Produces(StatusCodes.Status200OK, typeof(ShoppingListPushSyncResponse));

                return group;
            }

            /// <summary>
            /// Retorna alteracoes incrementais de listas de compras desde o cursor informado pelo cliente.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SyncShoppingListsEndpoint()
            {
                group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncShoppingListsRequest request, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    return Results.Ok(await shoppingListBLL.SyncAsync(userUid, request, cancellationToken));
                })
                .Produces(StatusCodes.Status200OK, typeof(ShoppingListSyncResponse));

                return group;
            }

            /// <summary>
            /// Obtem uma lista de compras especifica do usuario autenticado, com suporte a ETag do recurso.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetShoppingListEndpoint()
            {
                group.MapGet("/{shoppingListId:guid}", async (HttpContext httpContext, Guid shoppingListId, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var list = await shoppingListBLL.GetByIdAsync(userUid, shoppingListId, cancellationToken);
                    var etag = ShoppingListEtagHelper.BuildDetailsEtag(list);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(list);
                })
                .Produces(StatusCodes.Status200OK, typeof(ShoppingListDetailsResponse));

                return group;
            }

            /// <summary>
            /// Cria uma lista de compras para o usuario autenticado e devolve a representacao criada com ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder CreateShoppingListEndpoint()
            {
                group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateShoppingListRequest request, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var list = await shoppingListBLL.CreateAsync(userUid, request, cancellationToken);
                    httpContext.Response.Headers.ETag = ShoppingListEtagHelper.BuildDetailsEtag(list);
                    return Results.Created($"/api/v2/shopping/{list.Id}", list);
                })
                .Produces(StatusCodes.Status201Created, typeof(ShoppingListDetailsResponse));

                return group;
            }

            /// <summary>
            /// Atualiza uma lista de compras do usuario autenticado, respeitando If-Match para concorrencia otimista.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateShoppingListEndpoint()
            {
                group.MapPut("/{shoppingListId:guid}", async (HttpContext httpContext, Guid shoppingListId, [FromBody] UpdateShoppingListRequest request, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentList = await shoppingListBLL.GetByIdAsync(userUid, shoppingListId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, ShoppingListEtagHelper.BuildDetailsEtag(currentList), "A lista de compras foi alterada por outro cliente. Atualize os dados e tente novamente.");
                    var updatedList = await shoppingListBLL.UpdateAsync(userUid, shoppingListId, request, cancellationToken);
                    httpContext.Response.Headers.ETag = ShoppingListEtagHelper.BuildDetailsEtag(updatedList);
                    return Results.Ok(updatedList);
                })
                .Produces(StatusCodes.Status200OK, typeof(ShoppingListDetailsResponse));

                return group;
            }

            /// <summary>
            /// Remove logicamente uma lista de compras do usuario autenticado, mantendo tombstone para sync.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder DeleteShoppingListEndpoint()
            {
                group.MapDelete("/{shoppingListId:guid}", async (HttpContext httpContext, Guid shoppingListId, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentList = await shoppingListBLL.GetByIdAsync(userUid, shoppingListId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, ShoppingListEtagHelper.BuildDetailsEtag(currentList), "A lista de compras foi alterada por outro cliente. Atualize os dados e tente novamente.");
                    await shoppingListBLL.DeleteAsync(userUid, shoppingListId, cancellationToken);
                    return Results.NoContent();
                })
                .Produces(StatusCodes.Status204NoContent);

                return group;
            }

            #region Metodos de apoio para push sync offline-first
            private static async Task<ShoppingListPushSyncItemResponse> ProcessPushOperationAsync(Guid userUid, ShoppingListPushSyncItemRequest operation, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken)
            {
                try
                {
                    ValidateOperation(operation);

                    return operation.Operation switch
                    {
                        ShoppingListPushOperationType.Create => await ApplyCreateAsync(userUid, operation, shoppingListBLL, cancellationToken),
                        ShoppingListPushOperationType.Update => await ApplyUpdateAsync(userUid, operation, shoppingListBLL, cancellationToken),
                        ShoppingListPushOperationType.Delete => await ApplyDeleteAsync(userUid, operation, shoppingListBLL, cancellationToken),
                        _ => BuildFailure(operation, ShoppingListPushSyncItemStatus.ValidationError, "invalid_operation", "Operacao de push sync invalida.")
                    };
                }
                catch (ValidationException ex)
                {
                    return BuildFailure(operation, ShoppingListPushSyncItemStatus.ValidationError, "validation_error", ApiResourceHttpHelper.FlattenValidationErrors(ex));
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    return BuildFailure(operation, ShoppingListPushSyncItemStatus.Conflict, "conflict", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (operation.Operation == ShoppingListPushOperationType.Delete)
                    {
                        return new ShoppingListPushSyncItemResponse { ClientMutationId = operation.ClientMutationId, Status = ShoppingListPushSyncItemStatus.Applied };
                    }

                    return BuildFailure(operation, ShoppingListPushSyncItemStatus.NotFound, "not_found", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex)
                {
                    return BuildFailure(operation, ShoppingListPushSyncItemStatus.Failed, "api_error", ex.UserMessage ?? ex.Message);
                }
                catch (Exception ex)
                {
                    return BuildFailure(operation, ShoppingListPushSyncItemStatus.Failed, "unexpected_error", ex.Message);
                }
            }

            private static async Task<ShoppingListPushSyncItemResponse> ApplyCreateAsync(Guid userUid, ShoppingListPushSyncItemRequest operation, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken)
            {
                var list = await shoppingListBLL.CreateAsync(userUid, operation.Create!, cancellationToken);
                return new ShoppingListPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ShoppingListPushSyncItemStatus.Applied,
                    ShoppingList = list,
                    ServerEtag = ShoppingListEtagHelper.BuildDetailsEtag(list)
                };
            }

            private static async Task<ShoppingListPushSyncItemResponse> ApplyUpdateAsync(Guid userUid, ShoppingListPushSyncItemRequest operation, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken)
            {
                var currentList = await shoppingListBLL.GetByIdAsync(userUid, operation.ShoppingListId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentList);
                var list = await shoppingListBLL.UpdateAsync(userUid, operation.ShoppingListId.Value, operation.Update!, cancellationToken);
                return new ShoppingListPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ShoppingListPushSyncItemStatus.Applied,
                    ShoppingList = list,
                    ServerEtag = ShoppingListEtagHelper.BuildDetailsEtag(list)
                };
            }

            private static async Task<ShoppingListPushSyncItemResponse> ApplyDeleteAsync(Guid userUid, ShoppingListPushSyncItemRequest operation, IShoppingListBLL shoppingListBLL, CancellationToken cancellationToken)
            {
                var currentList = await shoppingListBLL.GetByIdAsync(userUid, operation.ShoppingListId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentList);
                await shoppingListBLL.DeleteAsync(userUid, operation.ShoppingListId.Value, cancellationToken);
                return new ShoppingListPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ShoppingListPushSyncItemStatus.Applied,
                    Deleted = new DeletedShoppingListResponse { Id = currentList.Id, DeletedAt = SaoPauloDateTime.Now() }
                };
            }

            private static void ValidateOperation(ShoppingListPushSyncItemRequest operation)
            {
                if (string.IsNullOrWhiteSpace(operation.ClientMutationId))
                {
                    throw new ValidationException("ClientMutationId", "O identificador da mutacao do cliente e obrigatorio.");
                }

                switch (operation.Operation)
                {
                    case ShoppingListPushOperationType.Create when operation.Create is null:
                        throw new ValidationException("Create", "O payload de criacao e obrigatorio.");
                    case ShoppingListPushOperationType.Update when operation.ShoppingListId is null || operation.Update is null:
                        throw new ValidationException("Update", "ShoppingListId e payload de atualizacao sao obrigatorios.");
                    case ShoppingListPushOperationType.Delete when operation.ShoppingListId is null:
                        throw new ValidationException("ShoppingListId", "ShoppingListId e obrigatorio para exclusao.");
                }
            }

            private static void EnsureExpectedEtag(string? expectedEtag, ShoppingListDetailsResponse currentList)
            {
                if (string.IsNullOrWhiteSpace(expectedEtag))
                {
                    return;
                }

                var currentEtag = ShoppingListEtagHelper.BuildDetailsEtag(currentList);
                if (!string.Equals(expectedEtag.Trim(), currentEtag, StringComparison.Ordinal) && expectedEtag.Trim() != "*")
                {
                    throw new ApiException(HttpStatusCode.PreconditionFailed, "A lista de compras foi alterada por outro cliente. Atualize os dados e tente novamente.");
                }
            }

            private static ShoppingListPushSyncItemResponse BuildFailure(ShoppingListPushSyncItemRequest operation, ShoppingListPushSyncItemStatus status, string errorCode, string errorMessage)
            {
                return new ShoppingListPushSyncItemResponse
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
