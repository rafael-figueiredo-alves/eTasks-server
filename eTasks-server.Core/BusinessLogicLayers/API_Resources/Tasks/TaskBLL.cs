using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.DTOs.Tasks.Responses;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Common;
using eTasks_server.Models.Enums.Tasks;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Tasks
{
    /// <summary>
    /// Regras de negócio do recurso de gerencionamento de Tarefas
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="logger">Serviço de log</param>
    public class TaskBLL(AppDbContext context, ILogger<ITaskBLL> logger) : BaseBLL<ITaskBLL>(context, logger), ITaskBLL
    {
        #region Funções principais
        /// <summary>
        /// Função destinada a obter a lista de tarefas
        /// </summary>
        /// <param name="userUid">Uid do usuário conectado</param>
        /// <param name="request">Parâmetros da requisição</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<TaskListItemResponse>> ListAsync(Guid userUid, ListTasksRequest request, CancellationToken cancellationToken = default)
        {
            // Obtém dados e valida usuário ativo
            await GetAndValidateActiveUserAsync(userUid);

            // Valida parâmetros da requisição
            ValidateListRequest(request);

            // Normaliza os parâmetros
            var effectiveRequest = NormalizeListRequest(request);

            // Verifica se foi solicitado incluir tarefas recursivas
            if (effectiveRequest.IncludeRecurring)
            {
                // Gera a relação de tarefas recursivas
                await MaterializeRecurringTasksAsync(userUid, effectiveRequest, cancellationToken);
            }

            // Obtém lista de tarefas, incluindo recorrência e apenas as pertinentes ao usuário que não estão apagadas
            var query = _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Se não for para incluir recorrência
            if (!effectiveRequest.IncludeRecurring)
            {
                query = query.Where(x =>
                    x.GeneratedFromTaskId == null &&
                    (x.Recurrence == null || !x.Recurrence.IsActive || x.Recurrence.RecurrenceType == RecurrenceType.None));
            }

            // Se foi passada uma data para obter a lista
            if (effectiveRequest.ReferenceDate.HasValue)
            {
                var referenceDate = effectiveRequest.ReferenceDate.Value.Date;
                query = query.Where(x => x.TaskDate.Date == referenceDate);
            }
            else
            {
                // Se foi passado um período para obter as tarefas
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

            // Se foi passado o filtro para apenas pegar tarefas concluídas
            if (effectiveRequest.IsCompleted.HasValue)
            {
                query = query.Where(x => x.IsCompleted == effectiveRequest.IsCompleted.Value);
            }

            // Se foi passado valor de filtro de prioridade
            if (effectiveRequest.Priority.HasValue)
            {
                query = query.Where(x => x.Priority == effectiveRequest.Priority.Value);
            }

            // Se foi feita consulta por termo específico
            if (!string.IsNullOrWhiteSpace(effectiveRequest.SearchTerm))
            {
                var searchTerm = effectiveRequest.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Summary.Contains(searchTerm) ||
                    (x.Notes != null && x.Notes.Contains(searchTerm)));
            }

            // Materializa a lista com ordenação por data e prioridade e por resumo da tarefa
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

        /// <summary>
        /// Obtém uma tarefa em específico buscando por seu ID
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="taskId">Id da tarefa</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskDetailsResponse> GetByIdAsync(Guid userUid, Guid taskId, CancellationToken cancellationToken = default)
        {
            // Valida o usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtém dados da tarefa
            var task = await _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            // Valida se a tarefa foi encontrada
            task = EnsureFound(task, "Tarefa não encontrada.");

            // Valida se a tarefa é do usuário informado
            EnsureOwnership(task.UserUid, userUid);

            // Mapeia dados para a resposta
            return MapDetails(task);
        }

        /// <summary>
        /// Método para criar nova tarefa
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Dados para tarefa</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskDetailsResponse> CreateAsync(Guid userUid, CreateTaskRequest request, CancellationToken cancellationToken = default)
        {
            // Obtém e valida usuário informado
            await GetAndValidateActiveUserAsync(userUid);

            // Valida parâmetros informados
            ValidateCreateRequest(request);

            // Valida ID gerado pelo cliente
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            // Gera nova tarefa com dados vindos do cliente
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

            // Verifica se tem recorrência e mapeia para gravar junto
            if (HasActiveRecurrence(request.Recurrence))
            {
                task.Recurrence = MapRecurrence(request.Recurrence!, task.TaskDate.Date);
            }

            // Executa a gravação dos dados em Transação, se uma der errada, nada é persistido
            await ExecuteInTransactionAsync(async () =>
            {
                await _context.TaskItems.AddAsync(task, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                // Valida se a tarefa foi completa, para atribuir pontos de bonificação
                if (task.IsCompleted)
                {
                    await AwardCompletionPointsAsync(task, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            // Retorna tarefa gravada
            return await GetByIdAsync(userUid, task.Id, cancellationToken);
        }

        /// <summary>
        /// Método para atualizar os dados de uma tarefa
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="taskId">Id da tarefa</param>
        /// <param name="request">Dados da tarefa</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskDetailsResponse> UpdateAsync(Guid userUid, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados da tarefa a atualizar
            ValidateUpdateRequest(request);

            // Tentar obter a tarefa informada
            var task = await _context.TaskItems
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            // Valida se a tarefa existe
            task = EnsureFound(task, "Tarefa não encontrada.");

            // Garante que a tarefa é do usuário informado
            EnsureOwnership(task.UserUid, userUid);

            // Valida se tarefa foi completada
            var wasCompleted = task.IsCompleted;

            // Preenche o resumo da tarefa
            task.Summary = request.Summary.Trim();
            task.Notes = NormalizeNotes(request.Notes);
            task.Priority = request.Priority;
            task.TaskDate = request.TaskDate.Date;
            task.IsCompleted = request.IsCompleted;
            task.CompletedAt = request.IsCompleted
                ? task.CompletedAt ?? SaoPauloDateTime.Now()
                : null;
            task.UpdatedAt = SaoPauloDateTime.Now();

            // Aplica recorrência
            ApplyRecurrenceUpdate(task, request.Recurrence);

            // Grava em transação para se uma parte falhar, dá rollback em tudo
            await ExecuteInTransactionAsync(async () =>
            {
                // Se tarefa conclída, atribui pontos ao usuário
                if (!wasCompleted && task.IsCompleted)
                {
                    await AwardCompletionPointsAsync(task, cancellationToken);
                }
                // Ou se for inverso, remove pontos
                else if (wasCompleted && !task.IsCompleted)
                {
                    await RevertCompletionPointsAsync(task, cancellationToken);
                }

                // Salva tudo
                await SaveChangesContextAsync(cancellationToken);
            });

            // retorna a tarefa
            return await GetByIdAsync(userUid, task.Id, cancellationToken);
        }

        /// <summary>
        /// Marca a tarefa como concluída
        /// </summary>
        /// <param name="userUid">Uid da Tarefa</param>
        /// <param name="taskId">Id da tarefa</param>
        /// <param name="isCompleted">Flag se está concluída</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskDetailsResponse> SetCompletionAsync(Guid userUid, Guid taskId, bool isCompleted, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem tarefa
            var task = await _context.TaskItems
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            // Valida se tarefa existe
            task = EnsureFound(task, "Tarefa não encontrada.");

            // Valida se tarefa pertence ao usuário
            EnsureOwnership(task.UserUid, userUid);

            // Se tarefa concluída
            if (task.IsCompleted == isCompleted)
            {
                // Retorna mapa dos detalhes da tarefa
                return MapDetails(task);
            }

            // registra se tarefa está concluída
            task.IsCompleted = isCompleted;
            task.CompletedAt = isCompleted ? SaoPauloDateTime.Now() : null;
            task.UpdatedAt = SaoPauloDateTime.Now();

            // Grava transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Se concluída dá pontos, se não remove
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

            // Retorna tarefa
            return await GetByIdAsync(userUid, task.Id, cancellationToken);
        }

        /// <summary>
        /// Remove uma tarefa
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="taskId">Id da tarefa</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid userUid, Guid taskId, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // tenta obter a tarefa
            var task = await _context.TaskItems
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == taskId && !x.IsDeleted, cancellationToken);

            // Verifica se tarefa existe
            task = EnsureFound(task, "Tarefa não encontrada.");

            // Garante que tarefa pertence ao usuário
            EnsureOwnership(task.UserUid, userUid);

            // Grava em transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Marca tarefas a marcar para deletar
                var tasksToDelete = new List<TaskItem> { task };

                // Pega tarefas recorrentes geradas da original
                if (task.GeneratedFromTaskId is null)
                {
                    var generatedTasks = await _context.TaskItems
                        .Where(x => x.GeneratedFromTaskId == task.Id && !x.IsDeleted)
                        .ToListAsync(cancellationToken);

                    tasksToDelete.AddRange(generatedTasks);
                }

                // Pega todos os IDs a deletar
                var taskIds = tasksToDelete.Select(x => x.Id).ToList();

                // Remove todos os bonus ganhos pelas tarefas
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

                // Marca a data de remoção e marca a tarefa como excluída
                var deletedAt = SaoPauloDateTime.Now();
                foreach (var currentTask in tasksToDelete)
                {
                    currentTask.IsDeleted = true;
                    currentTask.DeletedAt = deletedAt;
                    currentTask.UpdatedAt = deletedAt;
                }

                // Salva
                await SaveChangesContextAsync(cancellationToken);
            });
        }

        /// <summary>
        /// Sincronizar tarefas offline e online
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Parâmetros da requisição</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TaskSyncResponse> SyncAsync(Guid userUid, SyncTasksRequest request, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados da requisição
            ValidateSyncRequest(request);

            // Valida se é para incluir recorrência e pega janela de tempo definida
            if (request.IncludeRecurring && request.WindowStart.HasValue && request.WindowEnd.HasValue)
            {
                // Materializa tarefas recorrentes do período
                await MaterializeRecurringTasksInRangeAsync(userUid, request.WindowStart.Value.Date, request.WindowEnd.Value.Date, cancellationToken);
            }

            // Grava data de inicio
            var since = request.Since;

            // Registra data do servidor (horário de São Paulo)
            var serverTime = SaoPauloDateTime.Now();

            // Obtem registros do banco
            var upsertsQuery = _context.TaskItems
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Obtem registros removidos
            var deletedQuery = _context.TaskItems
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            // Verifica se foi passado dada a partir de
            if (since.HasValue)
            {
                // Pega inserções/edições desde e também os removidos no mesmo período           
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > since.Value);
            }

            // Obtem lista de inserções / edições ordenada por data
            var upserts = await upsertsQuery
                .OrderBy(x => x.TaskDate)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            // Obtém lista de tarefas removidas
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

            // gera resposta de sincronização
            return new TaskSyncResponse
            {
                ServerTime = serverTime,
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }
        #endregion

        #region Métodos privados
        /// <summary>
        /// Padroniza os parêmetros da requisição
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Valida os parâmetros da lista de requisição
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateListRequest(ListTasksRequest request)
        {
            if (request.ReferenceDate.HasValue && (request.DateFrom.HasValue || request.DateTo.HasValue))
            {
                throw new ValidationException("ReferenceDate", "Informe ReferenceDate ou DateFrom/DateTo, mas não ambos.");
            }

            if (request.DateFrom.HasValue && request.DateTo.HasValue && request.DateFrom.Value.Date > request.DateTo.Value.Date)
            {
                throw new ValidationException("DateTo", "A data final deve ser maior ou igual a data inicial.");
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
            {
                throw new ValidationException("SearchTerm", "O termo de pesquisa deve ter no máximo 200 caracteres.");
            }

            if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
            {
                throw new ValidationException("Priority", "Prioridade inválida.");
            }
        }

        /// <summary>
        /// Valida parâmetros de criação
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateCreateRequest(CreateTaskRequest request)
        {
            ValidateTaskPayload(request.Summary, request.Notes, request.Priority, request.TaskDate, request.Recurrence);
        }

        /// <summary>
        /// Valida parâmetros de atualização
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateUpdateRequest(UpdateTaskRequest request)
        {
            ValidateTaskPayload(request.Summary, request.Notes, request.Priority, request.TaskDate, request.Recurrence);
        }

        /// <summary>
        /// Valida o Payload da tarefa
        /// </summary>
        /// <param name="summary">Resumo</param>
        /// <param name="notes">Anotações</param>
        /// <param name="priority">Prioridade</param>
        /// <param name="taskDate">Data de tarefa</param>
        /// <param name="recurrence">Recorrência</param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateTaskPayload(string summary, string? notes, TaskPriority priority, DateTime taskDate, TaskRecurrenceRequest? recurrence)
        {
            // Valida se resumo estiver em branco
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new ValidationException("Summary", "O resumo da tarefa é obrigatório.");
            }

            // Valida o tamanho máximo do resumo
            if (summary.Trim().Length > 200)
            {
                throw new ValidationException("Summary", "O resumo da tarefa deve ter no máximo 200 caracteres.");
            }

            // Valida tamanho máximo das anotações
            if (!string.IsNullOrWhiteSpace(notes) && notes.Trim().Length > 4000)
            {
                throw new ValidationException("Notes", "As anotações devem ter no máximo 4000 caracteres.");
            }

            // Valida a prioridade
            if (!Enum.IsDefined(priority))
            {
                throw new ValidationException("Priority", "Prioridade inválida.");
            }

            // Valida data da tarefa
            if (taskDate == default)
            {
                throw new ValidationException("TaskDate", "A data da tarefa é obrigatória.");
            }

            // Valida recorrência
            ValidateRecurrence(recurrence, taskDate.Date);
        }

        /// <summary>
        /// Valida Id do cliente
        /// </summary>
        /// <param name="clientGeneratedId">Id gerado pelo cliente</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            // Valida se o valor não foi passado
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            // Verifica se o id já está em uso
            var idAlreadyExists = await _context.TaskItems.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (idAlreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Já existe uma tarefa com o identificador informado pelo cliente.");
            }
        }

        /// <summary>
        /// Valida a requisição de sincronização
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateSyncRequest(SyncTasksRequest request)
        {
            // Valida o período
            if (request.WindowStart.HasValue ^ request.WindowEnd.HasValue)
            {
                throw new ValidationException("WindowStart", "Informe WindowStart e WindowEnd juntos para sincronização com recorrências.");
            }

            if (request.WindowStart.HasValue && request.WindowEnd.HasValue && request.WindowStart.Value.Date > request.WindowEnd.Value.Date)
            {
                throw new ValidationException("WindowEnd", "A janela final da sincronização deve ser maior ou igual a inicial.");
            }
        }

        /// <summary>
        /// Valida a recorrência
        /// </summary>
        /// <param name="recurrence"></param>
        /// <param name="taskDate"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateRecurrence(TaskRecurrenceRequest? recurrence, DateTime taskDate)
        {
            // Valida se não há recorrência
            if (!HasActiveRecurrence(recurrence))
            {
                return;
            }

            // Valida data inicial
            if (recurrence!.StartsOn == default)
            {
                throw new ValidationException("Recurrence.StartsOn", "A data inicial da recorrência é obrigatória.");
            }

            // Valida se inicio da recoência é diferente da data da tarefa
            if (recurrence.StartsOn.Date != taskDate)
            {
                throw new ValidationException("Recurrence.StartsOn", "A data inicial da recorrência deve ser igual a data da tarefa.");
            }

            // Valida o intervalo
            if (recurrence.Interval < 1)
            {
                throw new ValidationException("Recurrence.Interval", "O intervalo da recorrência deve ser maior que zero.");
            }

            // Valida período final da recorrência
            if (recurrence.EndsOn.HasValue && recurrence.EndsOn.Value.Date < recurrence.StartsOn.Date)
            {
                throw new ValidationException("Recurrence.EndsOn", "A data final da recorrência deve ser maior ou igual a data inicial.");
            }

            // Valida tipo de recorrência
            if (!Enum.IsDefined(recurrence.RecurrenceType))
            {
                throw new ValidationException("Recurrence.RecurrenceType", "Tipo de recorrência inválido.");
            }

            // Determina tipo de recorrência
            switch (recurrence.RecurrenceType)
            {
                case RecurrenceType.Weekly:
                    if (recurrence.WeekDays == WeekDays.None)
                    {
                        throw new ValidationException("Recurrence.WeekDays", "Informe ao menos um dia da semana para recorrência semanal.");
                    }
                    break;

                case RecurrenceType.Monthly:
                    if (!recurrence.DayOfMonth.HasValue || recurrence.DayOfMonth.Value < 1 || recurrence.DayOfMonth.Value > 31)
                    {
                        throw new ValidationException("Recurrence.DayOfMonth", "Informe um dia do mês válido para recorrência mensal.");
                    }
                    break;

                case RecurrenceType.Yearly:
                    if (!recurrence.MonthOfYear.HasValue || recurrence.MonthOfYear.Value < 1 || recurrence.MonthOfYear.Value > 12)
                    {
                        throw new ValidationException("Recurrence.MonthOfYear", "Informe um mês válido para recorrência anual.");
                    }

                    if (!recurrence.DayOfMonth.HasValue || recurrence.DayOfMonth.Value < 1 || recurrence.DayOfMonth.Value > 31)
                    {
                        throw new ValidationException("Recurrence.DayOfMonth", "Informe um dia do mês válido para recorrência anual.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Determina se há recorrência ativa
        /// </summary>
        /// <param name="recurrence"></param>
        /// <returns></returns>
        private static bool HasActiveRecurrence(TaskRecurrenceRequest? recurrence)
        {
            return recurrence is not null && recurrence.RecurrenceType != RecurrenceType.None;
        }

        /// <summary>
        /// Normaliza as Anotações
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static string? NormalizeNotes(string? notes)
        {
            return string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        /// <summary>
        /// Mapeia recorrência
        /// </summary>
        /// <param name="request"></param>
        /// <param name="taskDate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Aplica atualização das recorr^^encias
        /// </summary>
        /// <param name="task"></param>
        /// <param name="recurrenceRequest"></param>
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

        /// <summary>
        /// Materializa as recorrências
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Materializa recorrência de tarefas em lote
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task MaterializeRecurringTasksInRangeAsync(Guid userUid, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                await MaterializeRecurringTasksForDateAsync(userUid, date, cancellationToken);
            }
        }

        /// <summary>
        /// Materializa recorrência para data
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="targetDate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Valida se recorrência existe
        /// </summary>
        /// <param name="baseTask"></param>
        /// <param name="recurrence"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Verifica se encontra recorrência diária
        /// </summary>
        /// <param name="recurrence"></param>
        /// <param name="startDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        private static bool MatchesDaily(TaskRecurrence recurrence, DateTime startDate, DateTime targetDate)
        {
            var days = (targetDate - startDate).Days;
            return days >= 0 && days % recurrence.Interval == 0;
        }

        /// <summary>
        /// Verifica se encontra recorrência semanal
        /// </summary>
        /// <param name="recurrence"></param>
        /// <param name="startDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Verifica se encontra recorrência mensal
        /// </summary>
        /// <param name="recurrence"></param>
        /// <param name="startDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        private static bool MatchesMonthly(TaskRecurrence recurrence, DateTime startDate, DateTime targetDate)
        {
            if (!recurrence.DayOfMonth.HasValue || targetDate.Day != recurrence.DayOfMonth.Value)
            {
                return false;
            }

            var months = ((targetDate.Year - startDate.Year) * 12) + targetDate.Month - startDate.Month;
            return months >= 0 && months % recurrence.Interval == 0;
        }

        /// <summary>
        /// Verifica se encontra recorrência anual
        /// </summary>
        /// <param name="recurrence"></param>
        /// <param name="startDate"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converte o dia da recorrência em dia da semana
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Método para atribuir pontos ao concluir tarefa
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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
                _logger.LogInformation("Nenhuma regra ativa de bônus para TaskCompletion foi encontrada. Tarefa {TaskId}.", task.Id);
                return;
            }

            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = task.UserUid,
                Points = rule.DefaultPoints,
                Source = BonusPointSource.TaskCompletion,
                SourceReferenceId = task.Id,
                Description = $"Conclusão da tarefa '{task.Summary}'."
            }, cancellationToken);
        }

        /// <summary>
        /// Método para remover pontos de bonus de tarefa desmarcada de concluída
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// MApeia detalhes da tarefa
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
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
        #endregion
    }
}
