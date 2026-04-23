using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Goals
{
    public class GoalBLL(AppDbContext context, ILogger<IGoalBLL> logger) : BaseBLL<IGoalBLL>(context, logger), IGoalBLL
    {
        public async Task<List<GoalListItemResponse>> ListAsync(Guid userUid, ListGoalsRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            NormalizeListRequest(request);
            ValidateListRequest(request);

            var query = _context.Goals
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type.Value);
            }

            if (request.Priority.HasValue)
            {
                query = query.Where(x => x.Priority == request.Priority.Value);
            }

            if (request.OnlyRewarded.HasValue)
            {
                query = request.OnlyRewarded.Value
                    ? query.Where(x => x.RewardPoints.HasValue && x.RewardPoints.Value > 0)
                    : query.Where(x => !x.RewardPoints.HasValue || x.RewardPoints.Value <= 0);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Summary.Contains(searchTerm) ||
                    (x.Description != null && x.Description.Contains(searchTerm)));
            }

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

        public async Task<GoalDetailsResponse> GetByIdAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var goal = await _context.Goals
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == goalId && !x.IsDeleted, cancellationToken);

            goal = EnsureFound(goal, "Meta nao encontrada.");
            EnsureOwnership(goal.UserUid, userUid);

            return MapDetails(goal);
        }

        public async Task<GoalDetailsResponse> CreateAsync(Guid userUid, CreateGoalRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Summary, request.Description, request.Type, request.Priority, request.RewardPoints, request.Status);
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

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

            await ExecuteInTransactionAsync(async () =>
            {
                await _context.Goals.AddAsync(goal, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                if (IsCompleted(goal.Status))
                {
                    await AwardCompletionPointsAsync(goal, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            return await GetByIdAsync(userUid, goal.Id, cancellationToken);
        }

        public async Task<GoalDetailsResponse> UpdateAsync(Guid userUid, Guid goalId, UpdateGoalRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Summary, request.Description, request.Type, request.Priority, request.RewardPoints, request.Status);

            var goal = await _context.Goals
                .FirstOrDefaultAsync(x => x.Id == goalId && !x.IsDeleted, cancellationToken);

            goal = EnsureFound(goal, "Meta nao encontrada.");
            EnsureOwnership(goal.UserUid, userUid);

            var wasCompleted = IsCompleted(goal.Status);

            goal.Summary = request.Summary.Trim();
            goal.Description = NormalizeDescription(request.Description);
            goal.Type = request.Type;
            goal.Priority = request.Priority;
            goal.RewardPoints = NormalizeRewardPoints(request.RewardPoints);
            goal.Status = request.Status;
            goal.UpdatedAt = SaoPauloDateTime.Now();

            await ExecuteInTransactionAsync(async () =>
            {
                if (!wasCompleted && IsCompleted(goal.Status))
                {
                    await AwardCompletionPointsAsync(goal, cancellationToken);
                }
                else if (wasCompleted && !IsCompleted(goal.Status))
                {
                    await RevertCompletionPointsAsync(goal, cancellationToken);
                }

                await SaveChangesContextAsync(cancellationToken);
            });

            return await GetByIdAsync(userUid, goal.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var goal = await _context.Goals
                .FirstOrDefaultAsync(x => x.Id == goalId && !x.IsDeleted, cancellationToken);

            goal = EnsureFound(goal, "Meta nao encontrada.");
            EnsureOwnership(goal.UserUid, userUid);

            await ExecuteInTransactionAsync(async () =>
            {
                await RevertCompletionPointsAsync(goal, cancellationToken);

                var deletedAt = SaoPauloDateTime.Now();
                goal.IsDeleted = true;
                goal.DeletedAt = deletedAt;
                goal.UpdatedAt = deletedAt;

                await SaveChangesContextAsync(cancellationToken);
            });
        }

        public async Task<GoalSyncResponse> SyncAsync(Guid userUid, SyncGoalsRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var since = request.Since;
            var serverTime = SaoPauloDateTime.Now();

            var upsertsQuery = _context.Goals
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            var deletedQuery = _context.Goals
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            if (since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > since.Value);
            }

            var upserts = await upsertsQuery
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var deleted = await deletedQuery
                .OrderBy(x => x.DeletedAt)
                .ThenBy(x => x.Id)
                .Select(x => new DeletedGoalResponse
                {
                    Id = x.Id,
                    DeletedAt = x.DeletedAt!.Value
                })
                .ToListAsync(cancellationToken);

            return new GoalSyncResponse
            {
                ServerTime = serverTime,
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }

        private static void NormalizeListRequest(ListGoalsRequest request)
        {
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        private static void ValidateListRequest(ListGoalsRequest request)
        {
            if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
            {
                throw new ValidationException("Status", "Status da meta invalido.");
            }

            if (request.Type.HasValue && !Enum.IsDefined(request.Type.Value))
            {
                throw new ValidationException("Type", "Tipo da meta invalido.");
            }

            if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
            {
                throw new ValidationException("Priority", "Prioridade invalida.");
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
            {
                throw new ValidationException("SearchTerm", "O termo de pesquisa deve ter no maximo 200 caracteres.");
            }
        }

        private static void ValidatePayload(string summary, string? description, GoalType type, TaskPriority priority, int? rewardPoints, GoalStatus status)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new ValidationException("Summary", "O resumo da meta e obrigatorio.");
            }

            if (summary.Trim().Length > 200)
            {
                throw new ValidationException("Summary", "O resumo da meta deve ter no maximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 4000)
            {
                throw new ValidationException("Description", "A descricao da meta deve ter no maximo 4000 caracteres.");
            }

            if (!Enum.IsDefined(type))
            {
                throw new ValidationException("Type", "Tipo da meta invalido.");
            }

            if (!Enum.IsDefined(priority))
            {
                throw new ValidationException("Priority", "Prioridade invalida.");
            }

            if (!Enum.IsDefined(status))
            {
                throw new ValidationException("Status", "Status da meta invalido.");
            }

            if (rewardPoints.HasValue && rewardPoints.Value < 0)
            {
                throw new ValidationException("RewardPoints", "A pontuacao da meta nao pode ser negativa.");
            }
        }

        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.Goals.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Ja existe uma meta com o identificador informado pelo cliente.");
            }
        }

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

            var points = goal.RewardPoints;

            if (!points.HasValue || points.Value <= 0)
            {
                var rule = await _context.BonusPointRules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Source == BonusPointSource.GoalCompletion && x.IsActive, cancellationToken);

                if (rule is null)
                {
                    _logger.LogInformation("Nenhuma recompensa fixa nem regra ativa de bonus para GoalCompletion foi encontrada. Meta {GoalId}.", goal.Id);
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
                Description = $"Conclusao da meta '{goal.Summary}'."
            }, cancellationToken);
        }

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

        private static bool IsCompleted(GoalStatus status)
        {
            return status == GoalStatus.Completed;
        }

        private static int? NormalizeRewardPoints(int? rewardPoints)
        {
            return rewardPoints.HasValue && rewardPoints.Value <= 0 ? null : rewardPoints;
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

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
    }
}
