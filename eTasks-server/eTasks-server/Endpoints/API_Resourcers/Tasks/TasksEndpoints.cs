using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.DTOs.Tasks.Responses;
using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Tasks
{
    public static class TasksEndpoints
    {
        public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/tasks")
                .WithTags("Tarefas")
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });

            group.MapGet("/", async (HttpContext httpContext, [AsParameters] ListTasksRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var tasks = await taskBLL.ListAsync(userUid, request, cancellationToken);
                var etag = BuildListEtag(tasks, request);
                if (RequestMatchesIfNoneMatch(httpContext.Request, etag))
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

            group.MapGet("/{taskId:guid}", async (HttpContext httpContext, Guid taskId, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var task = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                var etag = BuildDetailsEtag(task);
                if (RequestMatchesIfNoneMatch(httpContext.Request, etag))
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

            group.MapPost("/", async (HttpContext httpContext, [FromBody] CreateTaskRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var task = await taskBLL.CreateAsync(userUid, request, cancellationToken);
                httpContext.Response.Headers.ETag = BuildDetailsEtag(task);
                return Results.Created($"/api/v2/tasks/{task.Id}", task);
            })
            .WithName("CreateTask")
            .WithSummary("Cria uma nova tarefa para o usuario autenticado.")
            .WithDescription("Permite cadastrar tarefa simples ou recorrente. Se a tarefa ja nascer concluida, a regra de pontuacao de TaskCompletion pode ser aplicada.")
            .Produces(StatusCodes.Status201Created, typeof(TaskDetailsResponse))
            .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
            .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{taskId:guid}", async (HttpContext httpContext, Guid taskId, [FromBody] UpdateTaskRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentTask = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                EnsureIfMatch(httpContext.Request, BuildDetailsEtag(currentTask));

                var updatedTask = await taskBLL.UpdateAsync(userUid, taskId, request, cancellationToken);
                httpContext.Response.Headers.ETag = BuildDetailsEtag(updatedTask);
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

            group.MapPatch("/{taskId:guid}/completion", async (HttpContext httpContext, Guid taskId, [FromBody] SetTaskCompletionRequest request, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentTask = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                EnsureIfMatch(httpContext.Request, BuildDetailsEtag(currentTask));

                var updatedTask = await taskBLL.SetCompletionAsync(userUid, taskId, request.IsCompleted, cancellationToken);
                httpContext.Response.Headers.ETag = BuildDetailsEtag(updatedTask);
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

            group.MapDelete("/{taskId:guid}", async (HttpContext httpContext, Guid taskId, ITaskBLL taskBLL, CancellationToken cancellationToken) =>
            {
                var userUid = httpContext.User.GetRequiredUserUid();
                var currentTask = await taskBLL.GetByIdAsync(userUid, taskId, cancellationToken);
                EnsureIfMatch(httpContext.Request, BuildDetailsEtag(currentTask));

                await taskBLL.DeleteAsync(userUid, taskId, cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteTask")
            .WithSummary("Remove uma tarefa do usuario autenticado.")
            .WithDescription("Aceita If-Match para concorrencia otimista via ETag. Se a tarefa for a base de uma recorrencia, as ocorrencias geradas da serie tambem sao removidas.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse))
            .Produces(StatusCodes.Status412PreconditionFailed, typeof(ErrorResponse));

            return app;
        }

        private static string BuildListEtag(IEnumerable<TaskListItemResponse> tasks, ListTasksRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.ReferenceDate?.Date.ToString("yyyy-MM-dd") ?? string.Empty).Append('|');
            builder.Append(request.DateFrom?.Date.ToString("yyyy-MM-dd") ?? string.Empty).Append('|');
            builder.Append(request.DateTo?.Date.ToString("yyyy-MM-dd") ?? string.Empty).Append('|');
            builder.Append(request.IsCompleted?.ToString()).Append('|');
            builder.Append(request.Priority?.ToString()).Append('|');
            builder.Append(request.SearchTerm ?? string.Empty).Append('|');
            builder.Append(request.IncludeRecurring).Append('|');

            foreach (var task in tasks.OrderBy(x => x.TaskDate).ThenBy(x => x.Id))
            {
                builder.Append(task.Id).Append('|')
                    .Append(task.TaskDate.ToString("O")).Append('|')
                    .Append(task.IsCompleted).Append('|')
                    .Append(task.CompletedAt?.ToString("O")).Append('|')
                    .Append((int)task.Priority).Append('|')
                    .Append(task.Summary).Append('|')
                    .Append(task.HasRecurrence).Append(';');
            }

            return BuildQuotedHash(builder.ToString());
        }

        private static string BuildDetailsEtag(TaskDetailsResponse task)
        {
            var payload = string.Join("|",
                task.Id,
                task.UserUid,
                task.GeneratedFromTaskId,
                task.Summary,
                task.Notes,
                (int)task.Priority,
                task.TaskDate.ToString("O"),
                task.IsCompleted,
                task.CompletedAt?.ToString("O"),
                task.CreatedAt.ToString("O"),
                task.UpdatedAt?.ToString("O"),
                task.Recurrence?.Id,
                task.Recurrence?.RecurrenceType,
                task.Recurrence?.Interval,
                task.Recurrence?.WeekDays,
                task.Recurrence?.DayOfMonth,
                task.Recurrence?.MonthOfYear,
                task.Recurrence?.StartsOn.ToString("O") ?? string.Empty,
                task.Recurrence?.EndsOn?.ToString("O"),
                task.Recurrence?.LastGeneratedAt?.ToString("O"),
                task.Recurrence?.IsActive);

            return BuildQuotedHash(payload);
        }

        private static string BuildQuotedHash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }

        private static bool RequestMatchesIfNoneMatch(HttpRequest request, string currentEtag)
        {
            if (!request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var values))
            {
                return false;
            }

            return values.Any(value => string.Equals(value.Trim(), currentEtag, StringComparison.Ordinal) || value.Trim() == "*");
        }

        private static void EnsureIfMatch(HttpRequest request, string currentEtag)
        {
            if (!request.Headers.TryGetValue(HeaderNames.IfMatch, out var values) || values.Count == 0)
            {
                return;
            }

            var matched = values.Any(value => string.Equals(value.Trim(), currentEtag, StringComparison.Ordinal) || value.Trim() == "*");
            if (!matched)
            {
                throw new ApiException(HttpStatusCode.PreconditionFailed, "A tarefa foi alterada por outro cliente. Atualize os dados e tente novamente.");
            }
        }
    }
}
