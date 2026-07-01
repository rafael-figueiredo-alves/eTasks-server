using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;
using eTasks_server.Models.Enums.Finances;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Finances
{
    public static class FinancesEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de financas sob a rota base "/finances". Todas as operacoes exigem JWT Bearer,
            /// usam o usuario autenticado como escopo dos lancamentos e preservam os contratos offline-first de
            /// ETag, If-None-Match, If-Match, sync incremental e push sync.
            /// </summary>
            /// <returns>O proprio <see cref="IEndpointRouteBuilder"/> para encadeamento do bootstrap.</returns>
            public IEndpointRouteBuilder MapFinancesEndpoints()
            {
                var group = app.MapGroup("/finances")
                    .WithTags("Finanças")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.ListFinanceEntriesEndpoint()
                     .GetFinanceMonthSummaryEndpoint()
                     .PushSyncFinanceEntriesEndpoint()
                     .SyncFinanceEntriesEndpoint()
                     .GetFinanceEntryEndpoint()
                     .CreateFinanceEntryEndpoint()
                     .UpdateFinanceEntryEndpoint()
                     .DeleteFinanceEntryEndpoint();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Lista os lancamentos financeiros do usuario autenticado. Retorna ETag da colecao filtrada e
            /// responde 304 quando o cliente envia If-None-Match compativel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder ListFinanceEntriesEndpoint()
            {
                group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListFinanceEntriesRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var entries = await financeBLL.ListAsync(userUid, request, cancellationToken);
                    var etag = FinanceEtagHelper.BuildListEtag(entries, request);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(entries);
                })
                .WithDisplayName("Listar lançamentos financeiros")
                .WithDescription("Retorna a lista de lançamentos financeiros do usuário autenticado, com suporte a filtragem por data, tipo, categoria, etc. Responde com ETag da coleção filtrada e 304 Not Modified quando o cliente envia If-None-Match compatível.")
                .WithSummary("Lista os lançamentos financeiros do usuário autenticado, com suporte a filtros e ETag para cacheamento eficiente.")
                .Produces(StatusCodes.Status200OK, typeof(List<FinanceEntryListItemResponse>))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Retorna o resumo mensal de receitas, despesas e saldo do usuario autenticado.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetFinanceMonthSummaryEndpoint()
            {
                group.MapGet("/summary/{year:int}/{month:int}", async (HttpContext httpContext, int year, int month, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    return Results.Ok(await financeBLL.GetMonthSummaryAsync(userUid, year, month, cancellationToken));
                })
                .WithDisplayName("Obter resumo mensal de finanças")
                .WithDescription("Retorna o resumo mensal de receitas, despesas e saldo do usuário autenticado para o mês e ano especificados.")
                .WithSummary("Retorna o resumo mensal de receitas, despesas e saldo do usuário autenticado.")
                .Produces(StatusCodes.Status200OK, typeof(FinanceMonthSummaryResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Processa a outbox offline de financas. Cada mutacao e tratada de forma independente para que
            /// falhas parciais nao bloqueiem o restante do lote.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder PushSyncFinanceEntriesEndpoint()
            {
                group.MapPost("/push-sync", async (HttpContext httpContext, [FromBody] FinanceEntryPushSyncRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = new FinanceEntryPushSyncResponse { ServerTime = SaoPauloDateTime.Now() };

                    foreach (var operation in request.Operations ?? [])
                    {
                        response.Results.Add(await ProcessPushOperationAsync(userUid, operation, financeBLL, cancellationToken));
                    }

                    return Results.Ok(response);
                })
                .WithDisplayName("Push sync de lançamentos financeiros")
                .WithDescription("Processa a outbox offline de finanças do usuário autenticado. Cada mutação é tratada de forma independente para que falhas parciais não bloqueiem o restante do lote. Retorna o resultado individual de cada operação, incluindo erros de validação, conflitos de concorrência e outros problemas.")
                .WithSummary("Processa a outbox offline de finanças do usuário autenticado, aplicando as mutações e retornando o resultado individual de cada operação.")
                .Produces(StatusCodes.Status200OK, typeof(FinanceEntryPushSyncResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Retorna alteracoes incrementais de financas desde o cursor informado pelo cliente.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SyncFinanceEntriesEndpoint()
            {
                group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncFinanceEntriesRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    return Results.Ok(await financeBLL.SyncAsync(userUid, request, cancellationToken));
                })
                .WithSummary("Retorna as alterações incrementais de finanças do usuário autenticado desde o cursor informado, para suporte a sincronização offline-first.")
                .WithDisplayName("Sincronização incremental de lançamentos financeiros")
                .WithDescription("Retorna as alterações incrementais de finanças do usuário autenticado desde o cursor informado, incluindo lançamentos criados, atualizados e deletados. O cursor pode ser um timestamp ou um token específico que representa o estado da última sincronização do cliente. Essa operação é fundamental para suportar a sincronização offline-first de forma eficiente, permitindo que os clientes obtenham apenas as mudanças relevantes desde a última atualização.")
                .Produces(StatusCodes.Status200OK, typeof(FinanceEntrySyncResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Obtem um lancamento financeiro especifico do usuario autenticado, com suporte a ETag do recurso.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetFinanceEntryEndpoint()
            {
                group.MapGet("/{financeEntryId:guid}", async (HttpContext httpContext, Guid financeEntryId, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var entry = await financeBLL.GetByIdAsync(userUid, financeEntryId, cancellationToken);
                    var etag = FinanceEtagHelper.BuildDetailsEtag(entry);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(entry);
                })
                .WithDescription("Retorna os detalhes de um lançamento financeiro específico do usuário autenticado, identificado por seu ID. Responde com ETag do recurso e 304 Not Modified quando o cliente envia If-None-Match compatível, para otimizar cacheamento e reduzir tráfego desnecessário.")
                .WithSummary("Obtém os detalhes de um lançamento financeiro específico do usuário autenticado, com suporte a ETag para cacheamento eficiente.")
                .WithDisplayName("Obter detalhes de lançamento financeiro")
                .Produces(StatusCodes.Status200OK, typeof(FinanceEntryDetailsResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Cria um lancamento financeiro para o usuario autenticado e devolve a representacao criada com ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder CreateFinanceEntryEndpoint()
            {
                group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateFinanceEntryRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();

                    var entry = await financeBLL.CreateAsync(userUid, request, cancellationToken);

                    httpContext.Response.Headers.ETag = FinanceEtagHelper.BuildDetailsEtag(entry);

                    return Results.Created($"/api/v2/finances/{entry.Id}", entry);
                })
                .WithDisplayName("Criar lançamento financeiro")
                .WithDescription("Cria um lançamento financeiro para o usuário autenticado com os dados fornecidos. Retorna a representação do recurso criado, incluindo seu ID e ETag para futuras operações de cacheamento e concorrência otimista.")
                .WithSummary("Cria um lançamento financeiro para o usuário autenticado e retorna a representação criada com ETag para cacheamento e concorrência otimista.")
                .Produces(StatusCodes.Status201Created, typeof(FinanceEntryDetailsResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Atualiza um lancamento financeiro do usuario autenticado, respeitando If-Match para concorrencia otimista.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateFinanceEntryEndpoint()
            {
                group.MapPut("/{financeEntryId:guid}", async (HttpContext httpContext, Guid financeEntryId, [FromBody] UpdateFinanceEntryRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();

                    var currentEntry = await financeBLL.GetByIdAsync(userUid, financeEntryId, cancellationToken);

                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, FinanceEtagHelper.BuildDetailsEtag(currentEntry), "O lançamento financeiro foi alterado por outro cliente. Atualize os dados e tente novamente.");

                    var updatedEntry = await financeBLL.UpdateAsync(userUid, financeEntryId, request, cancellationToken);

                    httpContext.Response.Headers.ETag = FinanceEtagHelper.BuildDetailsEtag(updatedEntry);

                    return Results.Ok(updatedEntry);
                })
                .WithDisplayName("Atualizar lançamento financeiro")
                .WithDescription("Atualiza um lançamento financeiro para o usuário autenticado com os dados fornecidos. Retorna a representação do recurso atualizado, incluindo seu ETag para futuras operações de cacheamento e concorrência otimista.")
                .WithSummary("Atualiza um lançamento financeiro para o usuário autenticado e retorna a representação atualizada com ETag para cacheamento e concorrência otimista.")
                .Produces(StatusCodes.Status200OK, typeof(FinanceEntryDetailsResponse))                
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Remove logicamente um lancamento financeiro do usuario autenticado, mantendo tombstone para sync.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder DeleteFinanceEntryEndpoint()
            {
                group.MapDelete("/{financeEntryId:guid}", async (HttpContext httpContext, Guid financeEntryId, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();

                    var currentEntry = await financeBLL.GetByIdAsync(userUid, financeEntryId, cancellationToken);

                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, FinanceEtagHelper.BuildDetailsEtag(currentEntry), "O lançamento financeiro foi alterado por outro cliente. Atualize os dados e tente novamente.");

                    await financeBLL.DeleteAsync(userUid, financeEntryId, cancellationToken);

                    return Results.NoContent();
                })
                .WithSummary("Remove logicamente um lançamento financeiro do usuário autenticado, mantendo tombstone para sincronização offline-first.")
                .WithDisplayName("Excluir lançamento financeiro")
                .WithDescription("Remove logicamente um lançamento financeiro do usuário autenticado, mantendo um tombstone para que a exclusão seja sincronizada corretamente com clientes offline. Responde com 204 No Content quando a exclusão é bem-sucedida.")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            #region Metodos de apoio para push sync offline-first
            private static async Task<FinanceEntryPushSyncItemResponse> ProcessPushOperationAsync(Guid userUid, FinanceEntryPushSyncItemRequest operation, IFinanceBLL financeBLL, CancellationToken cancellationToken)
            {
                try
                {
                    ValidateOperation(operation);

                    return operation.Operation switch
                    {
                        FinanceEntryPushOperationType.Create => await ApplyCreateAsync(userUid, operation, financeBLL, cancellationToken),
                        FinanceEntryPushOperationType.Update => await ApplyUpdateAsync(userUid, operation, financeBLL, cancellationToken),
                        FinanceEntryPushOperationType.Delete => await ApplyDeleteAsync(userUid, operation, financeBLL, cancellationToken),
                        _ => BuildFailure(operation, FinanceEntryPushSyncItemStatus.ValidationError, "invalid_operation", "Operação de push sync inválida.")
                    };
                }
                catch (ValidationException ex)
                {
                    return BuildFailure(operation, FinanceEntryPushSyncItemStatus.ValidationError, "validation_error", ApiResourceHttpHelper.FlattenValidationErrors(ex));
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    return BuildFailure(operation, FinanceEntryPushSyncItemStatus.Conflict, "conflict", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (operation.Operation == FinanceEntryPushOperationType.Delete)
                    {
                        return new FinanceEntryPushSyncItemResponse { ClientMutationId = operation.ClientMutationId, Status = FinanceEntryPushSyncItemStatus.Applied };
                    }

                    return BuildFailure(operation, FinanceEntryPushSyncItemStatus.NotFound, "not_found", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex)
                {
                    return BuildFailure(operation, FinanceEntryPushSyncItemStatus.Failed, "api_error", ex.UserMessage ?? ex.Message);
                }
                catch (Exception ex)
                {
                    return BuildFailure(operation, FinanceEntryPushSyncItemStatus.Failed, "unexpected_error", ex.Message);
                }
            }

            private static async Task<FinanceEntryPushSyncItemResponse> ApplyCreateAsync(Guid userUid, FinanceEntryPushSyncItemRequest operation, IFinanceBLL financeBLL, CancellationToken cancellationToken)
            {
                var entry = await financeBLL.CreateAsync(userUid, operation.Create!, cancellationToken);
                return new FinanceEntryPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = FinanceEntryPushSyncItemStatus.Applied,
                    FinanceEntry = entry,
                    ServerEtag = FinanceEtagHelper.BuildDetailsEtag(entry)
                };
            }

            private static async Task<FinanceEntryPushSyncItemResponse> ApplyUpdateAsync(Guid userUid, FinanceEntryPushSyncItemRequest operation, IFinanceBLL financeBLL, CancellationToken cancellationToken)
            {
                var currentEntry = await financeBLL.GetByIdAsync(userUid, operation.FinanceEntryId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentEntry);
                var entry = await financeBLL.UpdateAsync(userUid, operation.FinanceEntryId.Value, operation.Update!, cancellationToken);
                return new FinanceEntryPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = FinanceEntryPushSyncItemStatus.Applied,
                    FinanceEntry = entry,
                    ServerEtag = FinanceEtagHelper.BuildDetailsEtag(entry)
                };
            }

            private static async Task<FinanceEntryPushSyncItemResponse> ApplyDeleteAsync(Guid userUid, FinanceEntryPushSyncItemRequest operation, IFinanceBLL financeBLL, CancellationToken cancellationToken)
            {
                var currentEntry = await financeBLL.GetByIdAsync(userUid, operation.FinanceEntryId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentEntry);
                await financeBLL.DeleteAsync(userUid, operation.FinanceEntryId.Value, cancellationToken);
                return new FinanceEntryPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = FinanceEntryPushSyncItemStatus.Applied,
                    Deleted = new DeletedFinanceEntryResponse { Id = currentEntry.Id, DeletedAt = SaoPauloDateTime.Now() }
                };
            }

            private static void ValidateOperation(FinanceEntryPushSyncItemRequest operation)
            {
                if (string.IsNullOrWhiteSpace(operation.ClientMutationId))
                {
                    throw new ValidationException("ClientMutationId", "O identificador da mutção do cliente e obrigatório.");
                }

                switch (operation.Operation)
                {
                    case FinanceEntryPushOperationType.Create when operation.Create is null:
                        throw new ValidationException("Create", "O payload de criação e obrigatório.");
                    case FinanceEntryPushOperationType.Update when operation.FinanceEntryId is null || operation.Update is null:
                        throw new ValidationException("Update", "FinanceEntryId e payload de atualização são obrigatórios.");
                    case FinanceEntryPushOperationType.Delete when operation.FinanceEntryId is null:
                        throw new ValidationException("FinanceEntryId", "FinanceEntryId é obrigatório para exclusão.");
                }
            }

            private static void EnsureExpectedEtag(string? expectedEtag, FinanceEntryDetailsResponse currentEntry)
            {
                if (string.IsNullOrWhiteSpace(expectedEtag))
                {
                    return;
                }

                var currentEtag = FinanceEtagHelper.BuildDetailsEtag(currentEntry);
                if (!string.Equals(expectedEtag.Trim(), currentEtag, StringComparison.Ordinal) && expectedEtag.Trim() != "*")
                {
                    throw new ApiException(HttpStatusCode.PreconditionFailed, "O lançamento financeiro foi alterado por outro cliente. Atualize os dados e tente novamente.");
                }
            }

            private static FinanceEntryPushSyncItemResponse BuildFailure(FinanceEntryPushSyncItemRequest operation, FinanceEntryPushSyncItemStatus status, string errorCode, string errorMessage)
            {
                return new FinanceEntryPushSyncItemResponse
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
