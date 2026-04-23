using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.DTOs.Tasks.Responses;
using eTasks_server.Models.Entities.Common;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Tasks
{
    public class TaskBLL(AppDbContext context, ILogger<ITaskBLL> logger) : BaseBLL<ITaskBLL>(context, logger), ITaskBLL
    {
        public async Task<List<TaskListItemResponse>> ListAsync(Guid userUid, ListTasksRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidateListRequest(request);

            var effectiveRequest = NormalizeListRequest(request);

            if (effectiveRequest.IncludeRecurring)
            {
                await MaterializeRecurringTasksAsync(userUid, effectiveRequest, cancellationToken);
            }

            var query = _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            if (!effectiveRequest.IncludeRecurring)
            {
                query = query.Where(x =>
                    x.GeneratedFromTaskId == null &&
                    (x.Recurrence == null || !x.Recurrence.IsActive || x.Recurrence.RecurrenceType == RecurrenceType.None));
            }

            if (effectiveRequest.ReferenceDate.HasValue)
            {
                var referenceDate = effectiveRequest.ReferenceDate.Value.Date;
                query = query.Where(x => x.TaskDate.Date == referenceDate);
            }
            else
            {
                if (effectiveRequest.DateFrom.HasValue)
                {
                    var from = effectiveRequest.DateFrom.Value.Date;
                    query = query.Where(x => x.TaskDate.Date >= from);
                }

                if (effectiveRequest.DateTo.HasValue)
                {
                    var to = effectiveRequest.DateTo.Value.Date;
                    query = query.Where(x => x.TaskDate.Date <= to);
                }
            }

            if (effectiveRequest.IsCompleted.HasValue)
            {
                query = query.Where(x => x.IsCompleted == effectiveRequest.IsCompleted.Value);
            }

            if (effectiveRequest.Priority.HasValue)
            {
                query = query.Where(x => x.Priority == effectiveRequest.Priority.Value);
            }

            if (!string.IsNullOrWhiteSpace(effectiveRequest.SearchTerm))
            {
                var searchTerm = effectiveRequest.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Summary.Contains(searchTerm) ||
                    (x.Notes != null && x.Notes.Contains(searchTerm)));
            }

            return await query
                .OrderBy(x => x.TaskDate)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.Summary)
                .Select(x => new TaskListItemResponse
                {
                    Id = x.Id,
                    Summary = x.Summary,
                    Notes = x.Notes,
                    Priority = x.Priority,
                    TaskDate = x.TaskDate,
                    IsCompleted = x.IsCompleted,
                    CompletedAt = x.CompletedAt,
                    HasRecurrence = x.GeneratedFromTaskId != null || (x.Recurrence != null && x.Recurrence.IsActive && x.Recurrence.RecurrenceType != RecurrenceType.None)
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<TaskDetailsResponse> GetByIdAsync(Guid userUid, Guid taskId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var task = await _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            task = EnsureFound(task, "Tarefa nao encontrada.");
            EnsureOwnership(task.UserUid, userUid);

            return MapDetails(task);
        }

        public async Task<TaskDetailsResponse> CreateAsync(Guid userUid, CreateTaskRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidateCreateRequest(request);
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            var task = new TaskItem
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Summary = request.Summary.Trim(),
                Notes = NormalizeNotes(request.Notes),
                Priority = request.Priority,
                TaskDate = request.TaskDate.Date,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? SaoPauloDateTime.Now() : null,
                CreatedAt = SaoPauloDateTime.Now(),
                UpdatedAt = null
            };

            if (HasActiveRecurrence(request.Recurrence))
            {
                task.Recurrence = MapRecurrence(request.Recurrence!, task.TaskDate.Date);
            }

            await ExecuteInTransactionAsync(async () =>
            {
                await _context.TaskItems.AddAsync(task, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                if (task.IsCompleted)
                {
                    await AwardCompletionPointsAsync(task, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            return await GetByIdAsync(userUid, task.Id, cancellationToken);
        }

        public async Task<TaskDetailsResponse> UpdateAsync(Guid userUid, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidateUpdateRequest(request);

            var task = await _context.TaskItems
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            task = EnsureFound(task, "Tarefa nao encontrada.");
            EnsureOwnership(task.UserUid, userUid);

            var wasCompleted = task.IsCompleted;

            task.Summary = request.Summary.Trim();
            task.Notes = NormalizeNotes(request.Notes);
            task.Priority = request.Priority;
            task.TaskDate = request.TaskDate.Date;
            task.IsCompleted = request.IsCompleted;
            task.CompletedAt = request.IsCompleted
                ? task.CompletedAt ?? SaoPauloDateTime.Now()
                : null;
            task.UpdatedAt = SaoPauloDateTime.Now();

            ApplyRecurrenceUpdate(task, request.Recurrence);

            await ExecuteInTransactionAsync(async () =>
            {
                if (!wasCompleted && task.IsCompleted)
                {
                    await AwardCompletionPointsAsync(task, cancellationToken);
                }
                else if (wasCompleted && !task.IsCompleted)
                {
                    await RevertCompletionPointsAsync(task, cancellationToken);
                }

                await SaveChangesContextAsync(cancellationToken);
            });

            return await GetByIdAsync(userUid, task.Id, cancellationToken);
        }

        public async Task<TaskDetailsResponse> SetCompletionAsync(Guid userUid, Guid taskId, bool isCompleted, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var task = await _context.TaskItems
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            task = EnsureFound(task, "Tarefa nao encontrada.");
            EnsureOwnership(task.UserUid, userUid);

            if (task.IsCompleted == isCompleted)
            {
                return MapDetails(task);
            }

            task.IsCompleted = isCompleted;
            task.CompletedAt = isCompleted ? SaoPauloDateTime.Now() : null;
            task.UpdatedAt = SaoPauloDateTime.Now();

            await ExecuteInTransactionAsync(async () =>
            {
                if (isCompleted)
                {
                    await AwardCompletionPointsAsync(task, cancellationToken);
                }
                else
                {
                    await RevertCompletionPointsAsync(task, cancellationToken);
                }

                await SaveChangesContextAsync(cancellationToken);
            });

            return await GetByIdAsync(userUid, task.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid userUid, Guid taskId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var task = await _context.TaskItems
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            task = EnsureFound(task, "Tarefa nao encontrada.");
            EnsureOwnership(task.UserUid, userUid);

            await ExecuteInTransactionAsync(async () =>
            {
                var tasksToDelete = new List<TaskItem> { task };

                if (task.GeneratedFromTaskId is null)
                {
                    var generatedTasks = await _context.TaskItems
                        .Where(x => x.GeneratedFromTaskId == task.Id && !x.IsDeleted)
                        .ToListAsync(cancellationToken);

                    tasksToDelete.AddRange(generatedTasks);
                }

                var taskIds = tasksToDelete.Select(x => x.Id).ToList();

                var bonusEntries = await _context.UserBonusPoints
                    .Where(x =>
                        x.UserUid == userUid &&
                        x.Source == BonusPointSource.TaskCompletion &&
                        x.SourceReferenceId.HasValue &&
                        taskIds.Contains(x.SourceReferenceId.Value))
                    .ToListAsync(cancellationToken);

                if (bonusEntries.Count > 0)
                {
                    _context.UserBonusPoints.RemoveRange(bonusEntries);
                }

                var deletedAt = SaoPauloDateTime.Now();
                foreach (var currentTask in tasksToDelete)
                {
                    currentTask.IsDeleted = true;
                    currentTask.DeletedAt = deletedAt;
                    currentTask.UpdatedAt = deletedAt;
                }

                await SaveChangesContextAsync(cancellationToken);
            });
        }

        public async Task<TaskSyncResponse> SyncAsync(Guid userUid, SyncTasksRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidateSyncRequest(request);

            if (request.IncludeRecurring && request.WindowStart.HasValue && request.WindowEnd.HasValue)
            {
                await MaterializeRecurringTasksInRangeAsync(userUid, request.WindowStart.Value.Date, request.WindowEnd.Value.Date, cancellationToken);
            }

            var since = request.Since;
            var serverTime = SaoPauloDateTime.Now();

            var upsertsQuery = _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            var deletedQuery = _context.TaskItems
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            if (since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > since.Value);
            }

            var upserts = await upsertsQuery
                .OrderBy(x => x.TaskDate)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var deleted = await deletedQuery
                .OrderBy(x => x.DeletedAt)
                .ThenBy(x => x.Id)
                .Select(x => new DeletedTaskResponse
                {
                    Id = x.Id,
                    GeneratedFromTaskId = x.GeneratedFromTaskId,
                    DeletedAt = x.DeletedAt!.Value
                })
                .ToListAsync(cancellationToken);

            return new TaskSyncResponse
            {
                ServerTime = serverTime,
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }

        private static ListTasksRequest NormalizeListRequest(ListTasksRequest request)
        {
            if (request.ReferenceDate is null && request.DateFrom is null && request.DateTo is null)
            {
                request.ReferenceDate = SaoPauloDateTime.Now().Date;
            }

            request.ReferenceDate = request.ReferenceDate?.Date;
            request.DateFrom = request.DateFrom?.Date;
            request.DateTo = request.DateTo?.Date;
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();

            return request;
        }

        private static void ValidateListRequest(ListTasksRequest request)
        {
            if (request.ReferenceDate.HasValue && (request.DateFrom.HasValue || request.DateTo.HasValue))
            {
                throw new ValidationException("ReferenceDate", "Informe ReferenceDate ou DateFrom/DateTo, mas nao ambos.");
            }

            if (request.DateFrom.HasValue && request.DateTo.HasValue && request.DateFrom.Value.Date > request.DateTo.Value.Date)
            {
                throw new ValidationException("DateTo", "A data final deve ser maior ou igual a data inicial.");
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
            {
                throw new ValidationException("SearchTerm", "O termo de pesquisa deve ter no maximo 200 caracteres.");
            }

            if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
            {
                throw new ValidationException("Priority", "Prioridade invalida.");
            }
        }

        private static void ValidateCreateRequest(CreateTaskRequest request)
        {
            ValidateTaskPayload(request.Summary, request.Notes, request.Priority, request.TaskDate, request.Recurrence);
        }

        private static void ValidateUpdateRequest(UpdateTaskRequest request)
        {
            ValidateTaskPayload(request.Summary, request.Notes, request.Priority, request.TaskDate, request.Recurrence);
        }

        private static void ValidateTaskPayload(string summary, string? notes, TaskPriority priority, DateTime taskDate, TaskRecurrenceRequest? recurrence)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new ValidationException("Summary", "O resumo da tarefa e obrigatorio.");
            }

            if (summary.Trim().Length > 200)
            {
                throw new ValidationException("Summary", "O resumo da tarefa deve ter no maximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(notes) && notes.Trim().Length > 4000)
            {
                throw new ValidationException("Notes", "As anotacoes devem ter no maximo 4000 caracteres.");
            }

            if (!Enum.IsDefined(priority))
            {
                throw new ValidationException("Priority", "Prioridade invalida.");
            }

            if (taskDate == default)
            {
                throw new ValidationException("TaskDate", "A data da tarefa e obrigatoria.");
            }

            ValidateRecurrence(recurrence, taskDate.Date);
        }

        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var idAlreadyExists = await _context.TaskItems.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (idAlreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Ja existe uma tarefa com o identificador informado pelo cliente.");
            }
        }

        private static void ValidateSyncRequest(SyncTasksRequest request)
        {
            if (request.WindowStart.HasValue ^ request.WindowEnd.HasValue)
            {
                throw new ValidationException("WindowStart", "Informe WindowStart e WindowEnd juntos para sincronizacao com recorrencias.");
            }

            if (request.WindowStart.HasValue && request.WindowEnd.HasValue && request.WindowStart.Value.Date > request.WindowEnd.Value.Date)
            {
                throw new ValidationException("WindowEnd", "A janela final da sincronizacao deve ser maior ou igual a inicial.");
            }
        }

        private static void ValidateRecurrence(TaskRecurrenceRequest? recurrence, DateTime taskDate)
        {
            if (!HasActiveRecurrence(recurrence))
            {
                return;
            }

            if (recurrence!.StartsOn == default)
            {
                throw new ValidationException("Recurrence.StartsOn", "A data inicial da recorrencia e obrigatoria.");
            }

            if (recurrence.StartsOn.Date != taskDate)
            {
                throw new ValidationException("Recurrence.StartsOn", "A data inicial da recorrencia deve ser igual a data da tarefa.");
            }

            if (recurrence.Interval < 1)
            {
                throw new ValidationException("Recurrence.Interval", "O intervalo da recorrencia deve ser maior que zero.");
            }

            if (recurrence.EndsOn.HasValue && recurrence.EndsOn.Value.Date < recurrence.StartsOn.Date)
            {
                throw new ValidationException("Recurrence.EndsOn", "A data final da recorrencia deve ser maior ou igual a data inicial.");
            }

            if (!Enum.IsDefined(recurrence.RecurrenceType))
            {
                throw new ValidationException("Recurrence.RecurrenceType", "Tipo de recorrencia invalido.");
            }

            switch (recurrence.RecurrenceType)
            {
                case RecurrenceType.Weekly:
                    if (recurrence.WeekDays == WeekDays.None)
                    {
                        throw new ValidationException("Recurrence.WeekDays", "Informe ao menos um dia da semana para recorrencia semanal.");
                    }
                    break;

                case RecurrenceType.Monthly:
                    if (!recurrence.DayOfMonth.HasValue || recurrence.DayOfMonth.Value < 1 || recurrence.DayOfMonth.Value > 31)
                    {
                        throw new ValidationException("Recurrence.DayOfMonth", "Informe um dia do mes valido para recorrencia mensal.");
                    }
                    break;

                case RecurrenceType.Yearly:
                    if (!recurrence.MonthOfYear.HasValue || recurrence.MonthOfYear.Value < 1 || recurrence.MonthOfYear.Value > 12)
                    {
                        throw new ValidationException("Recurrence.MonthOfYear", "Informe um mes valido para recorrencia anual.");
                    }

                    if (!recurrence.DayOfMonth.HasValue || recurrence.DayOfMonth.Value < 1 || recurrence.DayOfMonth.Value > 31)
                    {
                        throw new ValidationException("Recurrence.DayOfMonth", "Informe um dia do mes valido para recorrencia anual.");
                    }
                    break;
            }
        }

        private static bool HasActiveRecurrence(TaskRecurrenceRequest? recurrence)
        {
            return recurrence is not null && recurrence.RecurrenceType != RecurrenceType.None;
        }

        private static string? NormalizeNotes(string? notes)
        {
            return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        private static TaskRecurrence MapRecurrence(TaskRecurrenceRequest request, DateTime taskDate)
        {
            return new TaskRecurrence
            {
                RecurrenceType = request.RecurrenceType,
                Interval = request.Interval,
                WeekDays = request.WeekDays,
                DayOfMonth = request.DayOfMonth,
                MonthOfYear = request.MonthOfYear,
                StartsOn = request.StartsOn.Date,
                EndsOn = request.EndsOn?.Date,
                IsActive = request.IsActive,
                LastGeneratedAt = taskDate
            };
        }

        private static void ApplyRecurrenceUpdate(TaskItem task, TaskRecurrenceRequest? recurrenceRequest)
        {
            if (!HasActiveRecurrence(recurrenceRequest))
            {
                if (task.Recurrence is not null)
                {
                    task.Recurrence.IsActive = false;
                    task.Recurrence.RecurrenceType = RecurrenceType.None;
                    task.Recurrence.WeekDays = WeekDays.None;
                    task.Recurrence.DayOfMonth = null;
                    task.Recurrence.MonthOfYear = null;
                    task.Recurrence.StartsOn = task.TaskDate.Date;
                    task.Recurrence.EndsOn = null;
                    task.Recurrence.Interval = 1;
                }

                return;
            }

            if (task.Recurrence is null)
            {
                task.Recurrence = MapRecurrence(recurrenceRequest!, task.TaskDate.Date);
                return;
            }

            task.Recurrence.RecurrenceType = recurrenceRequest!.RecurrenceType;
            task.Recurrence.Interval = recurrenceRequest.Interval;
            task.Recurrence.WeekDays = recurrenceRequest.WeekDays;
            task.Recurrence.DayOfMonth = recurrenceRequest.DayOfMonth;
            task.Recurrence.MonthOfYear = recurrenceRequest.MonthOfYear;
            task.Recurrence.StartsOn = recurrenceRequest.StartsOn.Date;
            task.Recurrence.EndsOn = recurrenceRequest.EndsOn?.Date;
            task.Recurrence.IsActive = recurrenceRequest.IsActive;
        }

        private async Task MaterializeRecurringTasksAsync(Guid userUid, ListTasksRequest request, CancellationToken cancellationToken)
        {
            if (request.ReferenceDate.HasValue)
            {
                await MaterializeRecurringTasksForDateAsync(userUid, request.ReferenceDate.Value.Date, cancellationToken);
                return;
            }

            var startDate = request.DateFrom?.Date ?? SaoPauloDateTime.Now().Date;
            var endDate = request.DateTo?.Date ?? startDate;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                await MaterializeRecurringTasksForDateAsync(userUid, date, cancellationToken);
            }
        }

        private async Task MaterializeRecurringTasksInRangeAsync(Guid userUid, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                await MaterializeRecurringTasksForDateAsync(userUid, date, cancellationToken);
            }
        }

        private async Task MaterializeRecurringTasksForDateAsync(Guid userUid, DateTime targetDate, CancellationToken cancellationToken)
        {
            var baseTasks = await _context.TaskItems
                .Include(x => x.Recurrence)
                .Where(x =>
                    x.UserUid == userUid &&
                    !x.IsDeleted &&
                    x.GeneratedFromTaskId == null &&
                    x.Recurrence != null &&
                    x.Recurrence.IsActive)
                .ToListAsync(cancellationToken);

            var changed = false;

            foreach (var baseTask in baseTasks)
            {
                var recurrence = baseTask.Recurrence!;

                if (baseTask.TaskDate.Date == targetDate)
                {
                    continue;
                }

                if (!MatchesRecurrence(baseTask, recurrence, targetDate))
                {
                    continue;
                }

                var alreadyExists = await _context.TaskItems.AnyAsync(
                    x => x.GeneratedFromTaskId == baseTask.Id && x.TaskDate.Date == targetDate,
                    cancellationToken);

                if (alreadyExists)
                {
                    continue;
                }

                await _context.TaskItems.AddAsync(new TaskItem
                {
                    UserUid = userUid,
                    GeneratedFromTaskId = baseTask.Id,
                    Summary = baseTask.Summary,
                    Notes = baseTask.Notes,
                    Priority = baseTask.Priority,
                    TaskDate = targetDate,
                    IsCompleted = false,
                    CompletedAt = null,
                    CreatedAt = SaoPauloDateTime.Now(),
                    UpdatedAt = null
                }, cancellationToken);

                recurrence.LastGeneratedAt = targetDate;
                changed = true;
            }

            if (changed)
            {
                await SaveChangesContextAsync(cancellationToken);
            }
        }

        private static bool MatchesRecurrence(TaskItem baseTask, TaskRecurrence recurrence, DateTime targetDate)
        {
            var startDate = recurrence.StartsOn.Date;
            var effectiveDate = targetDate.Date;

            if (effectiveDate < startDate)
            {
                return false;
            }

            if (recurrence.EndsOn.HasValue && effectiveDate > recurrence.EndsOn.Value.Date)
            {
                return false;
            }

            return recurrence.RecurrenceType switch
            {
                RecurrenceType.Daily => MatchesDaily(recurrence, startDate, effectiveDate),
                RecurrenceType.Weekly => MatchesWeekly(recurrence, startDate, effectiveDate),
                RecurrenceType.Monthly => MatchesMonthly(recurrence, startDate, effectiveDate),
                RecurrenceType.Yearly => MatchesYearly(recurrence, startDate, effectiveDate),
                _ => false
            };
        }

        private static bool MatchesDaily(TaskRecurrence recurrence, DateTime startDate, DateTime targetDate)
        {
            var days = (targetDate - startDate).Days;
            return days >= 0 && days % recurrence.Interval == 0;
        }

        private static bool MatchesWeekly(TaskRecurrence recurrence, DateTime startDate, DateTime targetDate)
        {
            var weekDays = ToWeekDay(targetDate.DayOfWeek);
            if ((recurrence.WeekDays & weekDays) == 0)
            {
                return false;
            }

            var days = (targetDate - startDate).Days;
            var weeks = days / 7;
            return days >= 0 && weeks % recurrence.Interval == 0;
        }

        private static bool MatchesMonthly(TaskRecurrence recurrence, DateTime startDate, DateTime targetDate)
        {
            if (!recurrence.DayOfMonth.HasValue || targetDate.Day != recurrence.DayOfMonth.Value)
            {
                return false;
            }

            var months = ((targetDate.Year - startDate.Year) * 12) + targetDate.Month - startDate.Month;
            return months >= 0 && months % recurrence.Interval == 0;
        }

        private static bool MatchesYearly(TaskRecurrence recurrence, DateTime startDate, DateTime targetDate)
        {
            if (!recurrence.MonthOfYear.HasValue || !recurrence.DayOfMonth.HasValue)
            {
                return false;
            }

            if (targetDate.Month != recurrence.MonthOfYear.Value || targetDate.Day != recurrence.DayOfMonth.Value)
            {
                return false;
            }

            var years = targetDate.Year - startDate.Year;
            return years >= 0 && years % recurrence.Interval == 0;
        }

        private static WeekDays ToWeekDay(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => WeekDays.Sunday,
                DayOfWeek.Monday => WeekDays.Monday,
                DayOfWeek.Tuesday => WeekDays.Tuesday,
                DayOfWeek.Wednesday => WeekDays.Wednesday,
                DayOfWeek.Thursday => WeekDays.Thursday,
                DayOfWeek.Friday => WeekDays.Friday,
                DayOfWeek.Saturday => WeekDays.Saturday,
                _ => WeekDays.None
            };
        }

        private async Task AwardCompletionPointsAsync(TaskItem task, CancellationToken cancellationToken)
        {
            var existingPoint = await _context.UserBonusPoints
                .AnyAsync(x =>
                    x.UserUid == task.UserUid &&
                    x.Source == BonusPointSource.TaskCompletion &&
                    x.SourceReferenceId == task.Id,
                    cancellationToken);

            if (existingPoint)
            {
                return;
            }

            var rule = await _context.BonusPointRules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == BonusPointSource.TaskCompletion && x.IsActive, cancellationToken);

            if (rule is null)
            {
                _logger.LogInformation("Nenhuma regra ativa de bonus para TaskCompletion foi encontrada. Tarefa {TaskId}.", task.Id);
                return;
            }

            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = task.UserUid,
                Points = rule.DefaultPoints,
                Source = BonusPointSource.TaskCompletion,
                SourceReferenceId = task.Id,
                Description = $"Conclusao da tarefa '{task.Summary}'."
            }, cancellationToken);
        }

        private async Task RevertCompletionPointsAsync(TaskItem task, CancellationToken cancellationToken)
        {
            var entries = await _context.UserBonusPoints
                .Where(x =>
                    x.UserUid == task.UserUid &&
                    x.Source == BonusPointSource.TaskCompletion &&
                    x.SourceReferenceId == task.Id)
                .ToListAsync(cancellationToken);

            if (entries.Count == 0)
            {
                return;
            }

            _context.UserBonusPoints.RemoveRange(entries);
        }

        private static TaskDetailsResponse MapDetails(TaskItem task)
        {
            return new TaskDetailsResponse
            {
                Id = task.Id,
                UserUid = task.UserUid,
                GeneratedFromTaskId = task.GeneratedFromTaskId,
                Summary = task.Summary,
                Notes = task.Notes,
                Priority = task.Priority,
                TaskDate = task.TaskDate,
                IsCompleted = task.IsCompleted,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Recurrence = task.Recurrence is null || !task.Recurrence.IsActive || task.Recurrence.RecurrenceType == RecurrenceType.None ? null : new TaskRecurrenceResponse
                {
                    Id = task.Recurrence.Id,
                    RecurrenceType = task.Recurrence.RecurrenceType,
                    Interval = task.Recurrence.Interval,
                    WeekDays = task.Recurrence.WeekDays,
                    DayOfMonth = task.Recurrence.DayOfMonth,
                    MonthOfYear = task.Recurrence.MonthOfYear,
                    StartsOn = task.Recurrence.StartsOn,
                    EndsOn = task.Recurrence.EndsOn,
                    LastGeneratedAt = task.Recurrence.LastGeneratedAt,
                    IsActive = task.Recurrence.IsActive
                }
            };
        }
    }
}
