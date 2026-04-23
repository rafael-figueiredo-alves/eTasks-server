using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Notes
{
    public static class NotesEndpoints
    {
        public static IEndpointRouteBuilder MapNotesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/notes")
                .WithTags("Anotacoes")
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });

            group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListNotesRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var notes = await noteBLL.ListAsync(userUid, request, cancellationToken);
                var etag = NoteEtagHelper.BuildListEtag(notes, request);
                if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                {
                    return Results.StatusCode(StatusCodes.Status304NotModified);
                }

                httpContext.Response.Headers.ETag = etag;
                return Results.Ok(notes);
            })
            .WithName("ListNotes")
            .WithSummary("Lista as anotacoes do usuario autenticado.")
            .WithDescription("Retorna apenas anotacoes do usuario autenticado, com filtros por termo de busca, data de criacao e data de atualizacao. O endpoint retorna ETag e aceita If-None-Match para cache.")
            .Produces(StatusCodes.Status200OK, typeof(List<NoteListItemResponse>))
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/push-sync", async (HttpContext httpContext, [FromBody] NotePushSyncRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var response = new NotePushSyncResponse
                {
                    ServerTime = SaoPauloDateTime.Now()
                };

                foreach (var operation in request.Operations ?? [])
                {
                    response.Results.Add(await ProcessPushOperationAsync(userUid, operation, noteBLL, cancellationToken));
                }

                return Results.Ok(response);
            })
            .WithName("PushSyncNotes")
            .WithSummary("Processa em lote as mutacoes pendentes da outbox de anotacoes.")
            .WithDescription("Recebe operacoes de create, update e delete geradas offline pelo cliente. Cada item e processado individualmente com retorno de sucesso, conflito, validacao ou falha.")
            .Produces(StatusCodes.Status200OK, typeof(NotePushSyncResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncNotesRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var payload = await noteBLL.SyncAsync(userUid, request, cancellationToken);
                return Results.Ok(payload);
            })
            .WithName("SyncNotes")
            .WithSummary("Retorna alteracoes incrementais de anotacoes para sincronizacao offline-first.")
            .WithDescription("Usa o cursor Since para retornar upserts e tombstones desde a ultima sincronizacao do cliente.")
            .Produces(StatusCodes.Status200OK, typeof(NoteSyncResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{noteId:guid}", async (HttpContext httpContext, Guid noteId, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var note = await noteBLL.GetByIdAsync(userUid, noteId, cancellationToken);
                var etag = NoteEtagHelper.BuildDetailsEtag(note);
                if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                {
                    return Results.StatusCode(StatusCodes.Status304NotModified);
                }

                httpContext.Response.Headers.ETag = etag;
                return Results.Ok(note);
            })
            .WithName("GetNote")
            .WithSummary("Obtem os detalhes completos de uma anotacao.")
            .WithDescription("Retorna apenas a anotacao pertencente ao usuario autenticado. O endpoint retorna ETag e aceita If-None-Match para cache.")
            .Produces(StatusCodes.Status200OK, typeof(NoteDetailsResponse))
            .Produces(StatusCodes.Status304NotModified)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse));

            group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateNoteRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var note = await noteBLL.CreateAsync(userUid, request, cancellationToken);
                httpContext.Response.Headers.ETag = NoteEtagHelper.BuildDetailsEtag(note);
                return Results.Created($"/api/v2/notes/{note.Id}", note);
            })
            .WithName("CreateNote")
            .WithSummary("Cria uma nova anotacao para o usuario autenticado.")
            .WithDescription("Permite cadastrar anotacoes livres, inclusive com ClientGeneratedId para criacao offline com ID estavel no cliente.")
            .Produces(StatusCodes.Status201Created, typeof(NoteDetailsResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{noteId:guid}", async (HttpContext httpContext, Guid noteId, [FromBody] UpdateNoteRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentNote = await noteBLL.GetByIdAsync(userUid, noteId, cancellationToken);
                ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, NoteEtagHelper.BuildDetailsEtag(currentNote), "A anotacao foi alterada por outro cliente. Atualize os dados e tente novamente.");

                var updatedNote = await noteBLL.UpdateAsync(userUid, noteId, request, cancellationToken);
                httpContext.Response.Headers.ETag = NoteEtagHelper.BuildDetailsEtag(updatedNote);
                return Results.Ok(updatedNote);
            })
            .WithName("UpdateNote")
            .WithSummary("Edita uma anotacao do usuario autenticado.")
            .WithDescription("Aceita If-Match para concorrencia otimista via ETag.")
            .Produces(StatusCodes.Status200OK, typeof(NoteDetailsResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
            .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

            group.MapDelete("/{noteId:guid}", async (HttpContext httpContext, Guid noteId, INoteBLL noteBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentNote = await noteBLL.GetByIdAsync(userUid, noteId, cancellationToken);
                ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, NoteEtagHelper.BuildDetailsEtag(currentNote), "A anotacao foi alterada por outro cliente. Atualize os dados e tente novamente.");

                await noteBLL.DeleteAsync(userUid, noteId, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteNote")
            .WithSummary("Remove logicamente uma anotacao do usuario autenticado.")
            .WithDescription("Aceita If-Match para concorrencia otimista via ETag. A anotacao nao e apagada fisicamente: ela vira um tombstone para sincronizacao offline-first.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
            .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

            return app;
        }

        private static async Task<NotePushSyncItemResponse> ProcessPushOperationAsync(Guid userUid, NotePushSyncItemRequest operation, INoteBLL noteBLL, CancellationToken cancellationToken)
        {
            try
            {
                ValidatePushOperation(operation);

                return operation.Operation switch
                {
                    NotePushOperationType.Create => await ApplyCreateAsync(userUid, operation, noteBLL, cancellationToken),
                    NotePushOperationType.Update => await ApplyUpdateAsync(userUid, operation, noteBLL, cancellationToken),
                    NotePushOperationType.Delete => await ApplyDeleteAsync(userUid, operation, noteBLL, cancellationToken),
                    _ => BuildFailure(operation, NotePushSyncItemStatus.ValidationError, "invalid_operation", "Operacao de push sync invalida.")
                };
            }
            catch (ValidationException ex)
            {
                return BuildFailure(operation, NotePushSyncItemStatus.ValidationError, "validation_error", ApiResourceHttpHelper.FlattenValidationErrors(ex));
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return BuildFailure(operation, NotePushSyncItemStatus.Conflict, "conflict", ex.UserMessage ?? ex.Message);
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                if (operation.Operation == NotePushOperationType.Delete)
                {
                    return new NotePushSyncItemResponse
                    {
                        ClientMutationId = operation.ClientMutationId,
                        Status = NotePushSyncItemStatus.Applied
                    };
                }

                return BuildFailure(operation, NotePushSyncItemStatus.NotFound, "not_found", ex.UserMessage ?? ex.Message);
            }
            catch (ApiException ex)
            {
                return BuildFailure(operation, NotePushSyncItemStatus.Failed, "api_error", ex.UserMessage ?? ex.Message);
            }
            catch (Exception ex)
            {
                return BuildFailure(operation, NotePushSyncItemStatus.Failed, "unexpected_error", ex.Message);
            }
        }

        private static async Task<NotePushSyncItemResponse> ApplyCreateAsync(Guid userUid, NotePushSyncItemRequest operation, INoteBLL noteBLL, CancellationToken cancellationToken)
        {
            var note = await noteBLL.CreateAsync(userUid, operation.Create!, cancellationToken);
            return new NotePushSyncItemResponse
            {
                ClientMutationId = operation.ClientMutationId,
                Status = NotePushSyncItemStatus.Applied,
                Note = note,
                ServerEtag = NoteEtagHelper.BuildDetailsEtag(note)
            };
        }

        private static async Task<NotePushSyncItemResponse> ApplyUpdateAsync(Guid userUid, NotePushSyncItemRequest operation, INoteBLL noteBLL, CancellationToken cancellationToken)
        {
            var currentNote = await noteBLL.GetByIdAsync(userUid, operation.NoteId!.Value, cancellationToken);
            EnsureExpectedEtag(operation.ExpectedEtag, currentNote);

            var note = await noteBLL.UpdateAsync(userUid, operation.NoteId.Value, operation.Update!, cancellationToken);
            return new NotePushSyncItemResponse
            {
                ClientMutationId = operation.ClientMutationId,
                Status = NotePushSyncItemStatus.Applied,
                Note = note,
                ServerEtag = NoteEtagHelper.BuildDetailsEtag(note)
            };
        }

        private static async Task<NotePushSyncItemResponse> ApplyDeleteAsync(Guid userUid, NotePushSyncItemRequest operation, INoteBLL noteBLL, CancellationToken cancellationToken)
        {
            var currentNote = await noteBLL.GetByIdAsync(userUid, operation.NoteId!.Value, cancellationToken);
            EnsureExpectedEtag(operation.ExpectedEtag, currentNote);

            await noteBLL.DeleteAsync(userUid, operation.NoteId.Value, cancellationToken);
            return new NotePushSyncItemResponse
            {
                ClientMutationId = operation.ClientMutationId,
                Status = NotePushSyncItemStatus.Applied,
                Deleted = new DeletedNoteResponse
                {
                    Id = currentNote.Id,
                    DeletedAt = SaoPauloDateTime.Now()
                }
            };
        }

        private static void ValidatePushOperation(NotePushSyncItemRequest operation)
        {
            if (string.IsNullOrWhiteSpace(operation.ClientMutationId))
            {
                throw new ValidationException("ClientMutationId", "O identificador da mutacao do cliente e obrigatorio.");
            }

            switch (operation.Operation)
            {
                case NotePushOperationType.Create when operation.Create is null:
                    throw new ValidationException("Create", "O payload de criacao e obrigatorio.");
                case NotePushOperationType.Update when operation.NoteId is null || operation.Update is null:
                    throw new ValidationException("Update", "NoteId e payload de atualizacao sao obrigatorios.");
                case NotePushOperationType.Delete when operation.NoteId is null:
                    throw new ValidationException("NoteId", "NoteId e obrigatorio para exclusao.");
            }
        }

        private static void EnsureExpectedEtag(string? expectedEtag, NoteDetailsResponse currentNote)
        {
            if (string.IsNullOrWhiteSpace(expectedEtag))
            {
                return;
            }

            var currentEtag = NoteEtagHelper.BuildDetailsEtag(currentNote);
            if (!string.Equals(expectedEtag.Trim(), currentEtag, StringComparison.Ordinal) && expectedEtag.Trim() != "*")
            {
                throw new ApiException(HttpStatusCode.PreconditionFailed, "A anotacao foi alterada por outro cliente. Atualize os dados e tente novamente.");
            }
        }

        private static NotePushSyncItemResponse BuildFailure(NotePushSyncItemRequest operation, NotePushSyncItemStatus status, string errorCode, string errorMessage)
        {
            return new NotePushSyncItemResponse
            {
                ClientMutationId = operation.ClientMutationId,
                Status = status,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
        }
    }
}
