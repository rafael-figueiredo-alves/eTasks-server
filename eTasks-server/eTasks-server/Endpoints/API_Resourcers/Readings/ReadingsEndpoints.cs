using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Readings
{
    public static class ReadingsEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de leituras sob a rota base "/readings". Todas as operacoes exigem JWT Bearer,
            /// usam o usuario autenticado como escopo de dados e preservam os contratos offline-first com ETag,
            /// If-None-Match, If-Match, sync incremental e push sync.
            /// </summary>
            /// <returns>O proprio <see cref="IEndpointRouteBuilder"/> para encadeamento do bootstrap.</returns>
            public IEndpointRouteBuilder MapReadingsEndpoints()
            {
                var group = app.MapGroup("/readings")
                    .WithTags("Leituras")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.ListReadingsEndpoint()
                     .PushSyncReadingsEndpoint()
                     .SyncReadingsEndpoint()
                     .GetReadingEndpoint()
                     .CreateReadingEndpoint()
                     .UpdateReadingEndpoint()
                     .UpdateReadingProgressEndpoint()
                     .DeleteReadingEndpoint();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Lista as leituras do usuario autenticado. Retorna ETag da colecao filtrada e responde 304
            /// quando o cliente envia If-None-Match compativel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder ListReadingsEndpoint()
            {
                group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListReadingsRequest request, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var readings = await readingBLL.ListAsync(userUid, request, cancellationToken);
                    var etag = ReadingEtagHelper.BuildListEtag(readings, request);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(readings);
                })
                .Produces(StatusCodes.Status200OK, typeof(List<ReadingListItemResponse>))
                .Produces(StatusCodes.Status304NotModified);

                return group;
            }

            /// <summary>
            /// Processa a outbox offline de leituras. Cada mutacao e tratada de forma independente para que
            /// falhas parciais nao bloqueiem o restante do lote.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder PushSyncReadingsEndpoint()
            {
                group.MapPost("/push-sync", async (HttpContext httpContext, [FromBody] ReadingPushSyncRequest request, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = new ReadingPushSyncResponse { ServerTime = SaoPauloDateTime.Now() };

                    foreach (var operation in request.Operations ?? [])
                    {
                        response.Results.Add(await ProcessPushOperationAsync(userUid, operation, readingBLL, cancellationToken));
                    }

                    return Results.Ok(response);
                })
                .Produces(StatusCodes.Status200OK, typeof(ReadingPushSyncResponse));

                return group;
            }

            /// <summary>
            /// Retorna alteracoes incrementais de leituras desde o cursor informado pelo cliente.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SyncReadingsEndpoint()
            {
                group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncReadingsRequest request, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    return Results.Ok(await readingBLL.SyncAsync(userUid, request, cancellationToken));
                })
                .Produces(StatusCodes.Status200OK, typeof(ReadingSyncResponse));

                return group;
            }

            /// <summary>
            /// Obtem uma leitura especifica do usuario autenticado, com suporte a ETag do recurso.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetReadingEndpoint()
            {
                group.MapGet("/{readingId:guid}", async (HttpContext httpContext, Guid readingId, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var reading = await readingBLL.GetByIdAsync(userUid, readingId, cancellationToken);
                    var etag = ReadingEtagHelper.BuildDetailsEtag(reading);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(reading);
                })
                .Produces(StatusCodes.Status200OK, typeof(ReadingDetailsResponse))
                .Produces(StatusCodes.Status304NotModified);

                return group;
            }

            /// <summary>
            /// Cria uma leitura para o usuario autenticado e devolve a representacao criada com ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder CreateReadingEndpoint()
            {
                group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateReadingRequest request, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var reading = await readingBLL.CreateAsync(userUid, request, cancellationToken);
                    httpContext.Response.Headers.ETag = ReadingEtagHelper.BuildDetailsEtag(reading);
                    return Results.Created($"/api/v2/readings/{reading.Id}", reading);
                })
                .Produces(StatusCodes.Status201Created, typeof(ReadingDetailsResponse));

                return group;
            }

            /// <summary>
            /// Atualiza uma leitura do usuario autenticado, respeitando If-Match para concorrencia otimista.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateReadingEndpoint()
            {
                group.MapPut("/{readingId:guid}", async (HttpContext httpContext, Guid readingId, [FromBody] UpdateReadingRequest request, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentReading = await readingBLL.GetByIdAsync(userUid, readingId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, ReadingEtagHelper.BuildDetailsEtag(currentReading), "A leitura foi alterada por outro cliente. Atualize os dados e tente novamente.");
                    var updatedReading = await readingBLL.UpdateAsync(userUid, readingId, request, cancellationToken);
                    httpContext.Response.Headers.ETag = ReadingEtagHelper.BuildDetailsEtag(updatedReading);
                    return Results.Ok(updatedReading);
                })
                .Produces(StatusCodes.Status200OK, typeof(ReadingDetailsResponse));

                return group;
            }

            /// <summary>
            /// Atualiza apenas o progresso de leitura, preservando o mesmo controle de concorrencia por ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateReadingProgressEndpoint()
            {
                group.MapPatch("/{readingId:guid}/progress", async (HttpContext httpContext, Guid readingId, [FromBody] UpdateReadingProgressRequest request, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentReading = await readingBLL.GetByIdAsync(userUid, readingId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, ReadingEtagHelper.BuildDetailsEtag(currentReading), "A leitura foi alterada por outro cliente. Atualize os dados e tente novamente.");
                    var updatedReading = await readingBLL.UpdateProgressAsync(userUid, readingId, request, cancellationToken);
                    httpContext.Response.Headers.ETag = ReadingEtagHelper.BuildDetailsEtag(updatedReading);
                    return Results.Ok(updatedReading);
                })
                .Produces(StatusCodes.Status200OK, typeof(ReadingDetailsResponse));

                return group;
            }

            /// <summary>
            /// Remove logicamente uma leitura do usuario autenticado, mantendo tombstone para sync.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder DeleteReadingEndpoint()
            {
                group.MapDelete("/{readingId:guid}", async (HttpContext httpContext, Guid readingId, IReadingBLL readingBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentReading = await readingBLL.GetByIdAsync(userUid, readingId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, ReadingEtagHelper.BuildDetailsEtag(currentReading), "A leitura foi alterada por outro cliente. Atualize os dados e tente novamente.");
                    await readingBLL.DeleteAsync(userUid, readingId, cancellationToken);
                    return Results.NoContent();
                })
                .Produces(StatusCodes.Status204NoContent);

                return group;
            }

            #region Metodos de apoio para push sync offline-first
            private static async Task<ReadingPushSyncItemResponse> ProcessPushOperationAsync(Guid userUid, ReadingPushSyncItemRequest operation, IReadingBLL readingBLL, CancellationToken cancellationToken)
            {
                try
                {
                    ValidateOperation(operation);

                    return operation.Operation switch
                    {
                        ReadingPushOperationType.Create => await ApplyCreateAsync(userUid, operation, readingBLL, cancellationToken),
                        ReadingPushOperationType.Update => await ApplyUpdateAsync(userUid, operation, readingBLL, cancellationToken),
                        ReadingPushOperationType.UpdateProgress => await ApplyProgressAsync(userUid, operation, readingBLL, cancellationToken),
                        ReadingPushOperationType.Delete => await ApplyDeleteAsync(userUid, operation, readingBLL, cancellationToken),
                        _ => BuildFailure(operation, ReadingPushSyncItemStatus.ValidationError, "invalid_operation", "Operacao de push sync invalida.")
                    };
                }
                catch (ValidationException ex)
                {
                    return BuildFailure(operation, ReadingPushSyncItemStatus.ValidationError, "validation_error", ApiResourceHttpHelper.FlattenValidationErrors(ex));
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    return BuildFailure(operation, ReadingPushSyncItemStatus.Conflict, "conflict", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (operation.Operation == ReadingPushOperationType.Delete)
                    {
                        return new ReadingPushSyncItemResponse { ClientMutationId = operation.ClientMutationId, Status = ReadingPushSyncItemStatus.Applied };
                    }

                    return BuildFailure(operation, ReadingPushSyncItemStatus.NotFound, "not_found", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex)
                {
                    return BuildFailure(operation, ReadingPushSyncItemStatus.Failed, "api_error", ex.UserMessage ?? ex.Message);
                }
                catch (Exception ex)
                {
                    return BuildFailure(operation, ReadingPushSyncItemStatus.Failed, "unexpected_error", ex.Message);
                }
            }

            private static async Task<ReadingPushSyncItemResponse> ApplyCreateAsync(Guid userUid, ReadingPushSyncItemRequest operation, IReadingBLL readingBLL, CancellationToken cancellationToken)
            {
                var reading = await readingBLL.CreateAsync(userUid, operation.Create!, cancellationToken);
                return new ReadingPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ReadingPushSyncItemStatus.Applied,
                    Reading = reading,
                    ServerEtag = ReadingEtagHelper.BuildDetailsEtag(reading)
                };
            }

            private static async Task<ReadingPushSyncItemResponse> ApplyUpdateAsync(Guid userUid, ReadingPushSyncItemRequest operation, IReadingBLL readingBLL, CancellationToken cancellationToken)
            {
                var currentReading = await readingBLL.GetByIdAsync(userUid, operation.ReadingId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentReading);
                var reading = await readingBLL.UpdateAsync(userUid, operation.ReadingId.Value, operation.Update!, cancellationToken);
                return new ReadingPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ReadingPushSyncItemStatus.Applied,
                    Reading = reading,
                    ServerEtag = ReadingEtagHelper.BuildDetailsEtag(reading)
                };
            }

            private static async Task<ReadingPushSyncItemResponse> ApplyProgressAsync(Guid userUid, ReadingPushSyncItemRequest operation, IReadingBLL readingBLL, CancellationToken cancellationToken)
            {
                var currentReading = await readingBLL.GetByIdAsync(userUid, operation.ReadingId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentReading);
                var reading = await readingBLL.UpdateProgressAsync(userUid, operation.ReadingId.Value, operation.Progress!, cancellationToken);
                return new ReadingPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ReadingPushSyncItemStatus.Applied,
                    Reading = reading,
                    ServerEtag = ReadingEtagHelper.BuildDetailsEtag(reading)
                };
            }

            private static async Task<ReadingPushSyncItemResponse> ApplyDeleteAsync(Guid userUid, ReadingPushSyncItemRequest operation, IReadingBLL readingBLL, CancellationToken cancellationToken)
            {
                var currentReading = await readingBLL.GetByIdAsync(userUid, operation.ReadingId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentReading);
                await readingBLL.DeleteAsync(userUid, operation.ReadingId.Value, cancellationToken);
                return new ReadingPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = ReadingPushSyncItemStatus.Applied,
                    Deleted = new DeletedReadingResponse { Id = currentReading.Id, DeletedAt = SaoPauloDateTime.Now() }
                };
            }

            private static void ValidateOperation(ReadingPushSyncItemRequest operation)
            {
                if (string.IsNullOrWhiteSpace(operation.ClientMutationId))
                {
                    throw new ValidationException("ClientMutationId", "O identificador da mutacao do cliente e obrigatorio.");
                }

                switch (operation.Operation)
                {
                    case ReadingPushOperationType.Create when operation.Create is null:
                        throw new ValidationException("Create", "O payload de criacao e obrigatorio.");
                    case ReadingPushOperationType.Update when operation.ReadingId is null || operation.Update is null:
                        throw new ValidationException("Update", "ReadingId e payload de atualizacao sao obrigatorios.");
                    case ReadingPushOperationType.UpdateProgress when operation.ReadingId is null || operation.Progress is null:
                        throw new ValidationException("Progress", "ReadingId e payload de progresso sao obrigatorios.");
                    case ReadingPushOperationType.Delete when operation.ReadingId is null:
                        throw new ValidationException("ReadingId", "ReadingId e obrigatorio para exclusao.");
                }
            }

            private static void EnsureExpectedEtag(string? expectedEtag, ReadingDetailsResponse currentReading)
            {
                if (string.IsNullOrWhiteSpace(expectedEtag))
                {
                    return;
                }

                var currentEtag = ReadingEtagHelper.BuildDetailsEtag(currentReading);
                if (!string.Equals(expectedEtag.Trim(), currentEtag, StringComparison.Ordinal) && expectedEtag.Trim() != "*")
                {
                    throw new ApiException(HttpStatusCode.PreconditionFailed, "A leitura foi alterada por outro cliente. Atualize os dados e tente novamente.");
                }
            }

            private static ReadingPushSyncItemResponse BuildFailure(ReadingPushSyncItemRequest operation, ReadingPushSyncItemStatus status, string errorCode, string errorMessage)
            {
                return new ReadingPushSyncItemResponse
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
