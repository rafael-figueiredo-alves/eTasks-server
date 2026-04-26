using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Finances
{
    public static class FinancesEndpoints
    {
        public static IEndpointRouteBuilder MapFinancesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/finances")
                .WithTags("Financas")
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });

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
            .Produces(StatusCodes.Status200OK, typeof(List<FinanceEntryListItemResponse>));

            group.MapGet("/summary/{year:int}/{month:int}", async (HttpContext httpContext, int year, int month, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                return Results.Ok(await financeBLL.GetMonthSummaryAsync(userUid, year, month, cancellationToken));
            })
            .Produces(StatusCodes.Status200OK, typeof(FinanceMonthSummaryResponse));

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
            .Produces(StatusCodes.Status200OK, typeof(FinanceEntryPushSyncResponse));

            group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncFinanceEntriesRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                return Results.Ok(await financeBLL.SyncAsync(userUid, request, cancellationToken));
            })
            .Produces(StatusCodes.Status200OK, typeof(FinanceEntrySyncResponse));

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
            .Produces(StatusCodes.Status200OK, typeof(FinanceEntryDetailsResponse));

            group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateFinanceEntryRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var entry = await financeBLL.CreateAsync(userUid, request, cancellationToken);
                httpContext.Response.Headers.ETag = FinanceEtagHelper.BuildDetailsEtag(entry);
                return Results.Created($"/api/v2/finances/{entry.Id}", entry);
            })
            .Produces(StatusCodes.Status201Created, typeof(FinanceEntryDetailsResponse));

            group.MapPut("/{financeEntryId:guid}", async (HttpContext httpContext, Guid financeEntryId, [FromBody] UpdateFinanceEntryRequest request, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentEntry = await financeBLL.GetByIdAsync(userUid, financeEntryId, cancellationToken);
                ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, FinanceEtagHelper.BuildDetailsEtag(currentEntry), "O lancamento financeiro foi alterado por outro cliente. Atualize os dados e tente novamente.");
                var updatedEntry = await financeBLL.UpdateAsync(userUid, financeEntryId, request, cancellationToken);
                httpContext.Response.Headers.ETag = FinanceEtagHelper.BuildDetailsEtag(updatedEntry);
                return Results.Ok(updatedEntry);
            })
            .Produces(StatusCodes.Status200OK, typeof(FinanceEntryDetailsResponse));

            group.MapDelete("/{financeEntryId:guid}", async (HttpContext httpContext, Guid financeEntryId, IFinanceBLL financeBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentEntry = await financeBLL.GetByIdAsync(userUid, financeEntryId, cancellationToken);
                ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, FinanceEtagHelper.BuildDetailsEtag(currentEntry), "O lancamento financeiro foi alterado por outro cliente. Atualize os dados e tente novamente.");
                await financeBLL.DeleteAsync(userUid, financeEntryId, cancellationToken);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent);

            return app;
        }

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
                    _ => BuildFailure(operation, FinanceEntryPushSyncItemStatus.ValidationError, "invalid_operation", "Operacao de push sync invalida.")
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
                throw new ValidationException("ClientMutationId", "O identificador da mutacao do cliente e obrigatorio.");
            }

            switch (operation.Operation)
            {
                case FinanceEntryPushOperationType.Create when operation.Create is null:
                    throw new ValidationException("Create", "O payload de criacao e obrigatorio.");
                case FinanceEntryPushOperationType.Update when operation.FinanceEntryId is null || operation.Update is null:
                    throw new ValidationException("Update", "FinanceEntryId e payload de atualizacao sao obrigatorios.");
                case FinanceEntryPushOperationType.Delete when operation.FinanceEntryId is null:
                    throw new ValidationException("FinanceEntryId", "FinanceEntryId e obrigatorio para exclusao.");
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
                throw new ApiException(HttpStatusCode.PreconditionFailed, "O lancamento financeiro foi alterado por outro cliente. Atualize os dados e tente novamente.");
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
    }
}
