using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Endpoints.API_Resourcers;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.DTOs.Tasks.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers.Tasks
{
    public static class TasksEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints de tarefas sob a rota base "/tasks". Todas as operacoes exigem JWT Bearer,
            /// usam o usuario autenticado como escopo de dados e preservam os contratos offline-first com ETag,
            /// If-None-Match, If-Match, sync incremental, recorrencias e push sync.
            /// </summary>
            /// <returns>O proprio <see cref="IEndpointRouteBuilder"/> para encadeamento do bootstrap.</returns>
            public IEndpointRouteBuilder MapTasksEndpoints()
            {
                var group = app.MapGroup("/tasks")
                    .WithTags("Tarefas")
                    .RequireAuthorization(policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });

                group.ListTasksEndpoint()
                     .PushSyncTasksEndpoint()
                     .SyncTasksEndpoint()
                     .GetTaskEndpoint()
                     .CreateTaskEndpoint()
                     .UpdateTaskEndpoint()
                     .SetTaskCompletionEndpoint()
                     .DeleteTaskEndpoint();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Lista as tarefas do usuario autenticado. Retorna ETag da colecao filtrada e responde 304
            /// quando o cliente envia If-None-Match compativel.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder ListTasksEndpoint()
            {
                group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListTasksRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var tasks = await taskBLL.ListAsync(userUid, request, cancellationToken);
                    var etag = TaskEtagHelper.BuildListEtag(tasks, request);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(tasks);
                })
                .WithName("ListTasks")
                .WithSummary("Lista as tarefas do usuario autenticado para uma data ou intervalo informado.")
                .WithDescription("Retorna apenas tarefas do usuario autenticado. Quando IncludeRecurring=true, recorrencias podem ser materializadas no periodo consultado. O endpoint retorna ETag e aceita If-None-Match para cache.")
                .Produces(StatusCodes.Status200OK, typeof(List<TaskListItemResponse>))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Processa a outbox offline de tarefas. Cada mutacao e tratada de forma independente para que
            /// falhas parciais nao bloqueiem o restante do lote.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder PushSyncTasksEndpoint()
            {
                group.MapPost("/push-sync", async (HttpContext httpContext, [FromBody] TaskPushSyncRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var response = new TaskPushSyncResponse
                    {
                        ServerTime = SaoPauloDateTime.Now()
                    };

                    foreach (var operation in request.Operations ?? [])
                    {
                        response.Results.Add(await ProcessPushOperationAsync(userUid, operation, taskBLL, cancellationToken));
                    }

                    return Results.Ok(response);
                })
                .WithName("PushSyncTasks")
                .WithSummary("Processa em lote as mutacoes pendentes da outbox de tarefas.")
                .WithDescription("Recebe operacoes de create, update, conclusao e delete geradas offline pelo cliente. Cada item e processado individualmente com retorno de sucesso, conflito, validacao ou falha.")
                .Produces(StatusCodes.Status200OK, typeof(TaskPushSyncResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Retorna alteracoes incrementais desde o cursor informado pelo cliente, incluindo materializacao
            /// de recorrencias quando a janela solicita esse comportamento.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SyncTasksEndpoint()
            {
                group.MapPost("/sync", async (HttpContext httpContext, [FromBody] SyncTasksRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var payload = await taskBLL.SyncAsync(userUid, request, cancellationToken);
                    return Results.Ok(payload);
                })
                .WithName("SyncTasks")
                .WithSummary("Retorna alteracoes incrementais de tarefas para sincronizacao offline-first.")
                .WithDescription("Usa o cursor Since para retornar upserts e tombstones desde a ultima sincronizacao. Quando WindowStart e WindowEnd sao informados com IncludeRecurring=true, a API materializa ocorrencias recorrentes dentro da janela antes de montar a resposta.")
                .Produces(StatusCodes.Status200OK, typeof(TaskSyncResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Obtem uma tarefa especifica do usuario autenticado. Retorna ETag do recurso e aceita If-None-Match.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder GetTaskEndpoint()
            {
                group.MapGet("/{taskId:guid}", async (HttpContext httpContext, Guid taskId, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var task = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                    var etag = TaskEtagHelper.BuildDetailsEtag(task);
                    if (ApiResourceHttpHelper.RequestMatchesIfNoneMatch(httpContext.Request, etag))
                    {
                        return Results.StatusCode(StatusCodes.Status304NotModified);
                    }

                    httpContext.Response.Headers.ETag = etag;
                    return Results.Ok(task);
                })
                .WithName("GetTask")
                .WithSummary("Obtem os detalhes completos de uma tarefa.")
                .WithDescription("Retorna apenas a tarefa pertencente ao usuario autenticado. O endpoint retorna ETag e aceita If-None-Match para cache.")
                .Produces(StatusCodes.Status200OK, typeof(TaskDetailsResponse))
                .Produces(StatusCodes.Status304NotModified)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Cria uma tarefa para o usuario autenticado e devolve a representacao criada com ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder CreateTaskEndpoint()
            {
                group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateTaskRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var task = await taskBLL.CreateAsync(userUid, request, cancellationToken);
                    httpContext.Response.Headers.ETag = TaskEtagHelper.BuildDetailsEtag(task);
                    return Results.Created($"/api/v2/tasks/{task.Id}", task);
                })
                .WithName("CreateTask")
                .WithSummary("Cria uma nova tarefa para o usuario autenticado.")
                .WithDescription("Permite cadastrar tarefa simples ou recorrente. Se a tarefa ja nascer concluida, a regra de pontuacao de TaskCompletion pode ser aplicada.")
                .Produces(StatusCodes.Status201Created, typeof(TaskDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized);

                return group;
            }

            /// <summary>
            /// Atualiza uma tarefa do usuario autenticado, respeitando If-Match para concorrencia otimista.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder UpdateTaskEndpoint()
            {
                group.MapPut("/{taskId:guid}", async (HttpContext httpContext, Guid taskId, [FromBody] UpdateTaskRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentTask = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, TaskEtagHelper.BuildDetailsEtag(currentTask), "A tarefa foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    var updatedTask = await taskBLL.UpdateAsync(userUid, taskId, request, cancellationToken);
                    httpContext.Response.Headers.ETag = TaskEtagHelper.BuildDetailsEtag(updatedTask);
                    return Results.Ok(updatedTask);
                })
                .WithName("UpdateTask")
                .WithSummary("Edita uma tarefa do usuario autenticado.")
                .WithDescription("Aceita If-Match para concorrencia otimista via ETag. Se o estado de conclusao mudar, a pontuacao da tarefa e ajustada automaticamente.")
                .Produces(StatusCodes.Status200OK, typeof(TaskDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Marca ou desmarca uma tarefa como concluida, preservando concorrencia otimista por ETag.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder SetTaskCompletionEndpoint()
            {
                group.MapPatch("/{taskId:guid}/completion", async (HttpContext httpContext, Guid taskId, [FromBody] SetTaskCompletionRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentTask = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, TaskEtagHelper.BuildDetailsEtag(currentTask), "A tarefa foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    var updatedTask = await taskBLL.SetCompletionAsync(userUid, taskId, request.IsCompleted, cancellationToken);
                    httpContext.Response.Headers.ETag = TaskEtagHelper.BuildDetailsEtag(updatedTask);
                    return Results.Ok(updatedTask);
                })
                .WithName("SetTaskCompletion")
                .WithSummary("Marca ou desmarca uma tarefa como concluida.")
                .WithDescription("Aceita If-Match para concorrencia otimista via ETag. Ao concluir, o usuario recebe os pontos da regra TaskCompletion ativa. Ao desfazer a conclusao, esses pontos sao revertidos.")
                .Produces(StatusCodes.Status200OK, typeof(TaskDetailsResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Remove logicamente uma tarefa do usuario autenticado, mantendo tombstone para sync.
            /// </summary>
            /// <returns>O proprio <see cref="RouteGroupBuilder"/> para encadeamento.</returns>
            private RouteGroupBuilder DeleteTaskEndpoint()
            {
                group.MapDelete("/{taskId:guid}", async (HttpContext httpContext, Guid taskId, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
                {
                    var userUid = httpContext.User.GetRequiredUserUid();
                    var currentTask = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                    ApiResourceHttpHelper.EnsureIfMatch(httpContext.Request, TaskEtagHelper.BuildDetailsEtag(currentTask), "A tarefa foi alterada por outro cliente. Atualize os dados e tente novamente.");

                    await taskBLL.DeleteAsync(userUid, taskId, cancellationToken);
                    return Results.NoContent();
                })
                .WithName("DeleteTask")
                .WithSummary("Remove logicamente uma tarefa do usuario autenticado.")
                .WithDescription("Aceita If-Match para concorrencia otimista via ETag. A tarefa nao e apagada fisicamente: ela vira um tombstone para sincronizacao offline-first. Se a tarefa for a base de uma recorrencia, as ocorrencias geradas da serie tambem sao removidas logicamente.")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
                .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

                return group;
            }

            #region Metodos de apoio para push sync offline-first
            private static async Task<TaskPushSyncItemResponse> ProcessPushOperationAsync(Guid userUid, TaskPushSyncItemRequest operation, ITaskBLL taskBLL, CancellationToken cancellationToken)
            {
                try
                {
                    ValidatePushOperation(operation);

                    return operation.Operation switch
                    {
                        TaskPushOperationType.Create => await ApplyCreateAsync(userUid, operation, taskBLL, cancellationToken),
                        TaskPushOperationType.Update => await ApplyUpdateAsync(userUid, operation, taskBLL, cancellationToken),
                        TaskPushOperationType.SetCompletion => await ApplyCompletionAsync(userUid, operation, taskBLL, cancellationToken),
                        TaskPushOperationType.Delete => await ApplyDeleteAsync(userUid, operation, taskBLL, cancellationToken),
                        _ => BuildFailure(operation, TaskPushSyncItemStatus.ValidationError, "invalid_operation", "Operacao de push sync invalida.")
                    };
                }
                catch (ValidationException ex)
                {
                    return BuildFailure(operation, TaskPushSyncItemStatus.ValidationError, "validation_error", ApiResourceHttpHelper.FlattenValidationErrors(ex));
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    return BuildFailure(operation, TaskPushSyncItemStatus.Conflict, "conflict", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    if (operation.Operation == TaskPushOperationType.Delete)
                    {
                        return new TaskPushSyncItemResponse
                        {
                            ClientMutationId = operation.ClientMutationId,
                            Status = TaskPushSyncItemStatus.Applied
                        };
                    }

                    return BuildFailure(operation, TaskPushSyncItemStatus.NotFound, "not_found", ex.UserMessage ?? ex.Message);
                }
                catch (ApiException ex)
                {
                    return BuildFailure(operation, TaskPushSyncItemStatus.Failed, "api_error", ex.UserMessage ?? ex.Message);
                }
                catch (Exception ex)
                {
                    return BuildFailure(operation, TaskPushSyncItemStatus.Failed, "unexpected_error", ex.Message);
                }
            }

            private static async Task<TaskPushSyncItemResponse> ApplyCreateAsync(Guid userUid, TaskPushSyncItemRequest operation, ITaskBLL taskBLL, CancellationToken cancellationToken)
            {
                var task = await taskBLL.CreateAsync(userUid, operation.Create!, cancellationToken);
                return new TaskPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = TaskPushSyncItemStatus.Applied,
                    Task = task,
                    ServerEtag = TaskEtagHelper.BuildDetailsEtag(task)
                };
            }

            private static async Task<TaskPushSyncItemResponse> ApplyUpdateAsync(Guid userUid, TaskPushSyncItemRequest operation, ITaskBLL taskBLL, CancellationToken cancellationToken)
            {
                var currentTask = await taskBLL.GetByIdAsync(userUid, operation.TaskId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentTask);

                var task = await taskBLL.UpdateAsync(userUid, operation.TaskId.Value, operation.Update!, cancellationToken);
                return new TaskPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = TaskPushSyncItemStatus.Applied,
                    Task = task,
                    ServerEtag = TaskEtagHelper.BuildDetailsEtag(task)
                };
            }

            private static async Task<TaskPushSyncItemResponse> ApplyCompletionAsync(Guid userUid, TaskPushSyncItemRequest operation, ITaskBLL taskBLL, CancellationToken cancellationToken)
            {
                var currentTask = await taskBLL.GetByIdAsync(userUid, operation.TaskId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentTask);

                var task = await taskBLL.SetCompletionAsync(userUid, operation.TaskId.Value, operation.Completion!.IsCompleted, cancellationToken);
                return new TaskPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = TaskPushSyncItemStatus.Applied,
                    Task = task,
                    ServerEtag = TaskEtagHelper.BuildDetailsEtag(task)
                };
            }

            private static async Task<TaskPushSyncItemResponse> ApplyDeleteAsync(Guid userUid, TaskPushSyncItemRequest operation, ITaskBLL taskBLL, CancellationToken cancellationToken)
            {
                var currentTask = await taskBLL.GetByIdAsync(userUid, operation.TaskId!.Value, cancellationToken);
                EnsureExpectedEtag(operation.ExpectedEtag, currentTask);

                await taskBLL.DeleteAsync(userUid, operation.TaskId.Value, cancellationToken);
                return new TaskPushSyncItemResponse
                {
                    ClientMutationId = operation.ClientMutationId,
                    Status = TaskPushSyncItemStatus.Applied,
                    Deleted = new DeletedTaskResponse
                    {
                        Id = currentTask.Id,
                        GeneratedFromTaskId = currentTask.GeneratedFromTaskId,
                        DeletedAt = SaoPauloDateTime.Now()
                    }
                };
            }

            private static void ValidatePushOperation(TaskPushSyncItemRequest operation)
            {
                if (string.IsNullOrWhiteSpace(operation.ClientMutationId))
                {
                    throw new ValidationException("ClientMutationId", "O identificador da mutacao do cliente e obrigatorio.");
                }

                switch (operation.Operation)
                {
                    case TaskPushOperationType.Create when operation.Create is null:
                        throw new ValidationException("Create", "O payload de criacao e obrigatorio.");
                    case TaskPushOperationType.Update when operation.TaskId is null || operation.Update is null:
                        throw new ValidationException("Update", "TaskId e payload de atualizacao sao obrigatorios.");
                    case TaskPushOperationType.SetCompletion when operation.TaskId is null || operation.Completion is null:
                        throw new ValidationException("Completion", "TaskId e payload de conclusao sao obrigatorios.");
                    case TaskPushOperationType.Delete when operation.TaskId is null:
                        throw new ValidationException("TaskId", "TaskId e obrigatorio para exclusao.");
                }
            }

            private static void EnsureExpectedEtag(string? expectedEtag, TaskDetailsResponse currentTask)
            {
                if (string.IsNullOrWhiteSpace(expectedEtag))
                {
                    return;
                }

                var currentEtag = TaskEtagHelper.BuildDetailsEtag(currentTask);
                if (!string.Equals(expectedEtag.Trim(), currentEtag, StringComparison.Ordinal) && expectedEtag.Trim() != "*")
                {
                    throw new ApiException(HttpStatusCode.PreconditionFailed, "A tarefa foi alterada por outro cliente. Atualize os dados e tente novamente.");
                }
            }

            private static TaskPushSyncItemResponse BuildFailure(TaskPushSyncItemRequest operation, TaskPushSyncItemStatus status, string errorCode, string errorMessage)
            {
                return new TaskPushSyncItemResponse
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
