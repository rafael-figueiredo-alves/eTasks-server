using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;
using eTasks_server.Models.Enums.Notes;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Notes
{
    public static class NotesEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de anotacoes sob a rota base "/notes". Todas as operacoes exigem JWT Bearer,
            /// usam o usuario autenticado como escopo de dados e preservam os contratos offline-first com ETag,
            /// If-None-Match, If-Match, sync incremental e push sync.
            /// </summary>
            /// <returns>O proprio <see cref="IEndpointRouteBuilder"/> para encadeamento do bootstrap.</returns>
            public IEndpointRouteBuilder MapNotesEndpoints()
            {
                var group = app.MapGroup("/notes")
                    .WithTags("Anotações")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.ListNotesEndpoint()
                     .PushSyncNotesEndpoint()
                     .SyncNotesEndpoint()
                     .GetNoteEndpoint()
                     .CreateNoteEndpoint()
                     .UpdateNoteEndpoint()
                     .DeleteNoteEndpoint();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Lista as anotacoes do usuario autenticado. Retorna ETag da colecao filtrada e responde 304
            /// quando o cliente envia If-None-Match compativel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder ListNotesEndpoint()
            {
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
                .WithDisplayName("Listar Anotações")
                .WithSummary("Lista as anotações do usuário autenticado.")
                .WithDescription("Retorna apenas anotações do usuário autenticado, com filtros por termo de busca, data de criação e data de atualização. O endpoint retorna ETag e aceita If-None-Match para cache.")
                .Produces(StatusCodes.Status200OK, typeof(List<NoteListItemResponse>))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Processa a outbox offline do cliente. Cada mutacao e tratada de forma independente para que
            /// falhas parciais nao bloqueiem o restante do lote.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder PushSyncNotesEndpoint()
            {
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
                .WithDisplayName("Sincronização Push de Anotações")
                .WithSummary("Processa em lote as mutações pendentes da outbox de anotações.")
                .WithDescription("Recebe operações de create, update e delete geradas offline pelo cliente. Cada item é processado individualmente com retorno de sucesso, conflito, validação ou falha.")
                .Produces(StatusCodes.Status200OK, typeof(NotePushSyncResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Retorna alteracoes incrementais desde o cursor informado pelo cliente, mantendo o contrato
            /// offline-first de upserts e tombstones.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SyncNotesEndpoint()
            {
                group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncNotesRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    
                    var payload = await noteBLL.SyncAsync(userUid, request, cancellationToken);
                    
                    return Results.Ok(payload);
                })
                .WithName("SyncNotes")
                .WithDisplayName("Sincronização Incremental de Anotações")
                .WithSummary("Retorna alterações incrementais de anotações para sincronização offline-first.")
                .WithDescription("Usa o cursor Since para retornar upserts e tombstones desde a última sincronização do cliente.")
                .Produces(StatusCodes.Status200OK, typeof(NoteSyncResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Obtém uma anotação específica do usuário autenticado. Retorna ETag do recurso e aceita
            /// If-None-Match para reduzir tráfego quando o cliente já possui a versão atual.
            /// </summary>
            /// <returns>O próprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetNoteEndpoint()
            {
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
                .WithDisplayName("Obter Anotação")
                .WithSummary("Obtém os detalhes completos de uma anotação.")
                .WithDescription("Retorna apenas a anotação pertencente ao usuário autenticado. O endpoint retorna ETag e aceita If-None-Match para cache.")
                .Produces(StatusCodes.Status200OK, typeof(NoteDetailsResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Cria uma anotacao para o usuario autenticado e devolve a representacao criada com ETag.
            /// ClientGeneratedId continua permitido para criacao offline com identificador estavel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder CreateNoteEndpoint()
            {
                group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateNoteRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var note = await noteBLL.CreateAsync(userUid, request, cancellationToken);
                    httpContext.Response.Headers.ETag = NoteEtagHelper.BuildDetailsEtag(note);
                    return Results.Created($"/api/v2/notes/{note.Id}", note);
                })
                .WithName("CreateNote")
                .WithDisplayName("Criar Anotação")
                .WithSummary("Cria uma nova anotação para o usuário autenticado.")
                .WithDescription("Permite cadastrar anotações livres, inclusive com ClientGeneratedId para criação offline com ID estável no cliente.")
                .Produces(StatusCodes.Status201Created, typeof(NoteDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Atualiza uma anotacao do usuario autenticado. Quando o cliente envia If-Match, a operacao
            /// usa concorrencia otimista baseada no ETag atual do recurso.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateNoteEndpoint()
            {
                group.MapPut("/{noteId:guid}", async (HttpContext httpContext, Guid noteId, [FromBody] UpdateNoteRequest request, INoteBLL noteBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();

                    var currentNote = await noteBLL.GetByIdAsync(userUid, noteId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, NoteEtagHelper.BuildDetailsEtag(currentNote), "A anotação foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    var updatedNote = await noteBLL.UpdateAsync(userUid, noteId, request, cancellationToken);
                    httpContext.Response.Headers.ETag = NoteEtagHelper.BuildDetailsEtag(updatedNote);
                    return Results.Ok(updatedNote);
                })
                .WithName("UpdateNote")
                .WithDisplayName("Atualizar Anotação")
                .WithSummary("Edita uma anotação do usuário autenticado.")
                .WithDescription("Aceita If-Match para concorrência otimista via ETag.")
                .Produces(StatusCodes.Status200OK, typeof(NoteDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Remove logicamente uma anotacao do usuario autenticado. A exclusao gera tombstone para sync
            /// e respeita If-Match quando o cliente envia uma versao esperada.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder DeleteNoteEndpoint()
            {
                group.MapDelete("/{noteId:guid}", async (HttpContext httpContext, Guid noteId, INoteBLL noteBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();

                    var currentNote = await noteBLL.GetByIdAsync(userUid, noteId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, NoteEtagHelper.BuildDetailsEtag(currentNote), "A anotação foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    await noteBLL.DeleteAsync(userUid, noteId, cancellationToken);
                    return Results.NoContent();
                })
                .WithName("DeleteNote")
                .WithDisplayName("Excluir Anotação")
                .WithSummary("Remove logicamente uma anotação do usuário autenticado.")
                .WithDescription("Aceita If-Match para concorrência otimista via ETag. A anotação não é apagada fisicamente: ela vira um tombstone para sincronização offline-first.")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            #region Metodos de apoio para push sync offline-first
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
                        _ => BuildFailure(operation, NotePushSyncItemStatus.ValidationError, "invalid_operation", "Operação de push sync inválida.")
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
                    throw new ValidationException("ClientMutationId", "O identificador da mutação do cliente é obrigatório.");
                }

                switch (operation.Operation)
                {
                    case NotePushOperationType.Create when operation.Create is null:
                        throw new ValidationException("Create", "O payload de criação é obrigatório.");
                    case NotePushOperationType.Update when operation.NoteId is null || operation.Update is null:
                        throw new ValidationException("Update", "NoteId e payload de atualização são obrigatórios.");
                    case NotePushOperationType.Delete when operation.NoteId is null:
                        throw new ValidationException("NoteId", "NoteId é obrigatório para exclusão.");
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
                    throw new ApiException(HttpStatusCode.PreconditionFailed, "A anotação foi alterada por outro cliente. Atualize os dados e tente novamente.");
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
            #endregion
        }
    }
}
