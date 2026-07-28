using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;
using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Goals;
using eTasks_server.Models.Enums.Tasks;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Goals
{
    /// <summary>
    /// Regras de negocio para metas da API.
    /// </summary>
    public class GoalBLL(AppDbContext context, ILogger<IGoalBLL> logger) : BaseBLL<IGoalBLL>(context, logger), IGoalBLL
    {
        #region Funções principais
        /// <summary>
        /// Lista as metas do usuario com filtros opcionais.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Parametros de filtro.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de metas.</returns>
        public async Task<List<GoalListItemResponse>> ListAsync(Guid userUid, ListGoalsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida e obtem usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Normaliza filtros
            NormalizeListRequest(request);

            // Valida filtros
            ValidateListRequest(request);

            // Obtem lista de metas não removidas
            var query = _context.Goals
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Aplica filtros

            // Se tiver filtro de status, aplica
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            // Se tiver filtro de tipo
            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type.Value);
            }

            // Filtro por prioridade
            if (request.Priority.HasValue)
            {
                query = query.Where(x => x.Priority == request.Priority.Value);
            }

            // Por recompensa
            if (request.OnlyRewarded.HasValue)
            {
                query = request.OnlyRewarded.Value
                    ? query.Where(x => x.RewardPoints.HasValue && x.RewardPoints.Value > 0)
                    : query.Where(x => !x.RewardPoints.HasValue || x.RewardPoints.Value <= 0);
            }

            // Por termo buscado
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Summary.Contains(searchTerm) ||
                    (x.Description != null && x.Description.Contains(searchTerm)));
            }

            // Retorna lista
            return await query
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Priority)
                .ThenBy(x => x.Summary)
                .Select(x => new GoalListItemResponse
                {
                    Id = x.Id,
                    Summary = x.Summary,
                    Description = x.Description,
                    Type = x.Type,
                    Priority = x.Priority,
                    RewardPoints = x.RewardPoints,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retorna uma meta pelo identificador.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="goalId">Identificador da meta.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes da meta.</returns>
        public async Task<GoalDetailsResponse> GetByIdAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Pega Meta do id informado
            var goal = await _context.Goals
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == goalId && !x.IsDeleted, cancellationToken);

            // Verifica se objetivo existe
            goal = EnsureFound(goal, "Meta não encontrada.");

            // Valida propriedade da meta
            EnsureOwnership(goal.UserUid, userUid);

            // Retorna Meta
            return MapDetails(goal);
        }

        /// <summary>
        /// Cria uma nova meta.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Dados da meta.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes da meta criada.</returns>
        public async Task<GoalDetailsResponse> CreateAsync(Guid userUid, CreateGoalRequest request, CancellationToken cancellationToken = default)
        {
            // Valida e obtem usuário
            await GetAndValidateActiveUserAsync(userUid);

            // VAlida corpo de dados
            ValidatePayload(request.Summary, request.Description, request.Type, request.Priority, request.RewardPoints, request.Status);

            // Valida Id gerado pelo cliente offline
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            // Gera entidade a criar
            var goal = new Goal
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Summary = request.Summary.Trim(),
                Description = NormalizeDescription(request.Description),
                Type = request.Type,
                Priority = request.Priority,
                RewardPoints = NormalizeRewardPoints(request.RewardPoints),
                Status = request.Status,
                CreatedAt = SaoPauloDateTime.Now(),
                UpdatedAt = null
            };

            // Executa gravação em transaction
            await ExecuteInTransactionAsync(async () =>
            {
                // Adiciona entidade ao contexo e salva dados
                await _context.Goals.AddAsync(goal, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                // Se meta cumprida, atribui pontos
                if (IsCompleted(goal.Status))
                {
                    await AwardCompletionPointsAsync(goal, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });


            // Retorna meta
            return await GetByIdAsync(userUid, goal.Id, cancellationToken);
        }

        /// <summary>
        /// Atualiza uma meta existente.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="goalId">Identificador da meta.</param>
        /// <param name="request">Novos dados da meta.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes da meta atualizada.</returns>
        public async Task<GoalDetailsResponse> UpdateAsync(Guid userUid, Guid goalId, UpdateGoalRequest request, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida corpo de dados de edição
            ValidatePayload(request.Summary, request.Description, request.Type, request.Priority, request.RewardPoints, request.Status);

            // Obtem meta
            var goal = await _context.Goals
                .FirstOrDefaultAsync(x => x.Id == goalId && !x.IsDeleted, cancellationToken);

            // Valida que a mesma exista
            goal = EnsureFound(goal, "Meta não encontrada.");

            // Valida se usuário possui a meta
            EnsureOwnership(goal.UserUid, userUid);

            // Guarda status anterior
            var wasCompleted = IsCompleted(goal.Status);

            // Edita dados da meta
            goal.Summary = request.Summary.Trim();
            goal.Description = NormalizeDescription(request.Description);
            goal.Type = request.Type;
            goal.Priority = request.Priority;
            goal.RewardPoints = NormalizeRewardPoints(request.RewardPoints);
            goal.Status = request.Status;
            goal.UpdatedAt = SaoPauloDateTime.Now();

            // Executa operações em transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Se tarefa foi concluída agora
                if (!wasCompleted && IsCompleted(goal.Status))
                {
                    // Atribui pontos
                    await AwardCompletionPointsAsync(goal, cancellationToken);
                }
                else if (wasCompleted && !IsCompleted(goal.Status))
                {
                    // Caso contrário, revoga pontos atribuídos antes
                    await RevertCompletionPointsAsync(goal, cancellationToken);
                }

                // Salva tudo
                await SaveChangesContextAsync(cancellationToken);
            });

            // Retorna meta
            return await GetByIdAsync(userUid, goal.Id, cancellationToken);
        }

        /// <summary>
        /// Remove logicamente uma meta.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="goalId">Identificador da meta.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task DeleteAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem meta pelo id informado
            var goal = await _context.Goals
                .FirstOrDefaultAsync(x => x.Id == goalId && !x.IsDeleted, cancellationToken);

            // Garante que meta existe
            goal = EnsureFound(goal, "Meta não encontrada.");

            // Garante que é do usuário
            EnsureOwnership(goal.UserUid, userUid);

            // Executa em transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Revoga pontos
                await RevertCompletionPointsAsync(goal, cancellationToken);

                // Marca remoção
                var deletedAt = SaoPauloDateTime.Now();
                goal.IsDeleted = true;
                goal.DeletedAt = deletedAt;
                goal.UpdatedAt = deletedAt;

                // Salva
                await SaveChangesContextAsync(cancellationToken);
            });
        }

        /// <summary>
        /// Sincroniza metas alteradas desde uma data base.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Parametros de sincronizacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resposta de sincronizacao com upserts e deletados.</returns>
        public async Task<GoalSyncResponse> SyncAsync(Guid userUid, SyncGoalsRequest request, CancellationToken cancellationToken = default)
        {
            // Obtem e salva usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Pega data de inicio da sincronização
            var since = request.Since;

            // Pega data/hora do servidor
            var serverTime = SaoPauloDateTime.Now();

            // Pega inserções/edições
            var upsertsQuery = _context.Goals
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Pega remoções
            var deletedQuery = _context.Goals
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            // Valida se é para buscar tudo a partir de data informada
            if (since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > since.Value);
            }

            // Retorna lista de edições/inserções
            var upserts = await upsertsQuery
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            // Retorna remoções
            var deleted = await deletedQuery
                .OrderBy(x => x.DeletedAt)
                .ThenBy(x => x.Id)
                .Select(x => new DeletedGoalResponse
                {
                    Id = x.Id,
                    DeletedAt = x.DeletedAt!.Value
                })
                .ToListAsync(cancellationToken);

            // Retorna lista de operações de sincronização a realizar
            return new GoalSyncResponse
            {
                ServerTime = serverTime,
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }
        #endregion

        #region Funções Auxiliares
        /// <summary>
        /// Normaliza campos de filtro antes da consulta.
        /// </summary>
        /// <param name="request">Parametros de listagem.</param>
        private static void NormalizeListRequest(ListGoalsRequest request)
        {
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        /// <summary>
        /// Valida os filtros aplicados na listagem de metas.
        /// </summary>
        /// <param name="request">Parametros de listagem.</param>
        private static void ValidateListRequest(ListGoalsRequest request)
        {
            if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
            {
                throw new ValidationException("Status", "Status da meta inválido.");
            }

            if (request.Type.HasValue && !Enum.IsDefined(request.Type.Value))
            {
                throw new ValidationException("Type", "Tipo da meta inválido.");
            }

            if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
            {
                throw new ValidationException("Priority", "Prioridade inválida.");
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
            {
                throw new ValidationException("SearchTerm", "O termo de pesquisa deve ter no máximo 200 caracteres.");
            }
        }

        /// <summary>
        /// Valida o payload de criacao ou atualizacao de meta.
        /// </summary>
        /// <param name="summary">Resumo da meta.</param>
        /// <param name="description">Descricao opcional.</param>
        /// <param name="type">Tipo da meta.</param>
        /// <param name="priority">Prioridade da meta.</param>
        /// <param name="rewardPoints">Pontuacao opcional.</param>
        /// <param name="status">Status da meta.</param>
        private static void ValidatePayload(string summary, string? description, GoalType type, TaskPriority priority, int? rewardPoints, GoalStatus status)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new ValidationException("Summary", "O resumo da meta é obrigatório.");
            }

            if (summary.Trim().Length > 200)
            {
                throw new ValidationException("Summary", "O resumo da meta deve ter no máximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 4000)
            {
                throw new ValidationException("Description", "A descrição da meta deve ter no máximo 4000 caracteres.");
            }

            if (!Enum.IsDefined(type))
            {
                throw new ValidationException("Type", "Tipo da meta inválido.");
            }

            if (!Enum.IsDefined(priority))
            {
                throw new ValidationException("Priority", "Prioridade inválida.");
            }

            if (!Enum.IsDefined(status))
            {
                throw new ValidationException("Status", "Status da meta inválido.");
            }

            if (rewardPoints.HasValue && rewardPoints.Value < 0)
            {
                throw new ValidationException("RewardPoints", "A pontuação da meta não pode ser negativa.");
            }
        }

        /// <summary>
        /// Garante que o identificador informado pelo cliente ainda nao exista.
        /// </summary>
        /// <param name="clientGeneratedId">Identificador opcional informado pelo cliente.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.Goals.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Já existe uma meta com o identificador informado pelo cliente.");
            }
        }

        /// <summary>
        /// Adiciona pontos de bonus quando a meta e concluida.
        /// </summary>
        /// <param name="goal">Meta alvo.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        private async Task AwardCompletionPointsAsync(Goal goal, CancellationToken cancellationToken)
        {
            var alreadyExists = await _context.UserBonusPoints
                .AnyAsync(x =>
                    x.UserUid == goal.UserUid &&
                    x.Source == BonusPointSource.GoalCompletion &&
                    x.SourceReferenceId == goal.Id,
                    cancellationToken);

            if (alreadyExists)
            {
                return;
            }

            // Usa a pontuacao customizada da meta ou a regra padrao ativa.
            var points = goal.RewardPoints;

            if (!points.HasValue || points.Value <= 0)
            {
                var rule = await _context.BonusPointRules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Source == BonusPointSource.GoalCompletion && x.IsActive, cancellationToken);

                if (rule is null)
                {
                    _logger.LogInformation("Nenhuma recompensa fixa nem regra ativa de bônus para GoalCompletion foi encontrada. Meta {GoalId}.", goal.Id);
                    return;
                }

                points = rule.DefaultPoints;
            }

            if (points.Value <= 0)
            {
                return;
            }

            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = goal.UserUid,
                Points = points.Value,
                Source = BonusPointSource.GoalCompletion,
                SourceReferenceId = goal.Id,
                Description = $"Conclusão da meta '{goal.Summary}'."
            }, cancellationToken);
        }

        /// <summary>
        /// Remove os pontos de bonus associados a uma meta concluida.
        /// </summary>
        /// <param name="goal">Meta alvo.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        private async Task RevertCompletionPointsAsync(Goal goal, CancellationToken cancellationToken)
        {
            var entries = await _context.UserBonusPoints
                .Where(x =>
                    x.UserUid == goal.UserUid &&
                    x.Source == BonusPointSource.GoalCompletion &&
                    x.SourceReferenceId == goal.Id)
                .ToListAsync(cancellationToken);

            if (entries.Count == 0)
            {
                return;
            }

            _context.UserBonusPoints.RemoveRange(entries);
        }

        /// <summary>
        /// Verifica se o status representa uma meta concluida.
        /// </summary>
        /// <param name="status">Status da meta.</param>
        /// <returns>True quando o status for Completed.</returns>
        private static bool IsCompleted(GoalStatus status)
        {
            return status == GoalStatus.Completed;
        }

        /// <summary>
        /// Normaliza a pontuacao customizada da meta.
        /// </summary>
        /// <param name="rewardPoints">Pontuacao enviada.</param>
        /// <returns>Pontuacao validada ou null.</returns>
        private static int? NormalizeRewardPoints(int? rewardPoints)
        {
            return rewardPoints.HasValue && rewardPoints.Value <= 0 ? null : rewardPoints;
        }

        /// <summary>
        /// Normaliza uma descricao opcional.
        /// </summary>
        /// <param name="description">Descricao original.</param>
        /// <returns>Descricao trimada ou null.</returns>
        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        /// <summary>
        /// Mapeia a entidade de meta para a resposta detalhada.
        /// </summary>
        /// <param name="goal">Entidade carregada do banco.</param>
        /// <returns>Resposta de detalhes da meta.</returns>
        private static GoalDetailsResponse MapDetails(Goal goal)
        {
            return new GoalDetailsResponse
            {
                Id = goal.Id,
                UserUid = goal.UserUid,
                Summary = goal.Summary,
                Description = goal.Description,
                Type = goal.Type,
                Priority = goal.Priority,
                RewardPoints = goal.RewardPoints,
                Status = goal.Status,
                CreatedAt = goal.CreatedAt,
                UpdatedAt = goal.UpdatedAt
            };
        }
        #endregion
    }
}
