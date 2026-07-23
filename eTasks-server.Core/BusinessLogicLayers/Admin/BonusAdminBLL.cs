using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Users;
using eTasks_server.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para conquistas e regras de pontos de bonus do painel administrativo.
    /// </summary>
    public class BonusAdminBLL(AppDbContext context, ILogger<IBonusAdminBLL> logger) : BaseBLL<IBonusAdminBLL>(context, logger), IBonusAdminBLL
    {
        #region Bonus Achievements

        /// <summary>
        /// Retorna a lista de conquistas cadastradas.
        /// </summary>
        /// <returns>Lista de conquistas de bonus.</returns>
        public async Task<List<BonusAchievementDTO>> GetAchievementsAsync()
        {
            return await _context.BonusAchievements
                .OrderBy(a => a.PointsRequired)
                .Select(a => new BonusAchievementDTO
                {
                    Id = a.Id,
                    Code = a.Code,
                    Name = a.Name,
                    Description = a.Description,
                    PointsRequired = a.PointsRequired,
                    DisplayType = (int)a.DisplayType,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma conquista de bonus pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da conquista.</param>
        /// <returns>Dados da conquista de bonus.</returns>
        public async Task<BonusAchievementDTO> GetAchievementAsync(Guid id)
        {
            var a = EnsureFound(await _context.BonusAchievements.FirstOrDefaultAsync(x => x.Id == id));
            return new BonusAchievementDTO
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                PointsRequired = a.PointsRequired,
                DisplayType = (int)a.DisplayType,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            };
        }

        /// <summary>
        /// Cria uma nova conquista de bonus.
        /// </summary>
        /// <param name="request">Dados da conquista.</param>
        /// <returns>Conquista criada.</returns>
        public async Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request)
        {
            EnsureUnique(await _context.BonusAchievements.AnyAsync(x => x.Code == request.Code), "Code", $"Já existe uma conquista com o código '{request.Code}'.");

            var achievement = new BonusAchievement
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                PointsRequired = request.PointsRequired,
                DisplayType = (AchievementDisplayType)request.DisplayType,
                IsActive = request.IsActive
            };

            await _context.BonusAchievements.AddAsync(achievement);
            await SaveChangesContextAsync();

            return await GetAchievementAsync(achievement.Id);
        }

        /// <summary>
        /// Atualiza uma conquista de bonus existente.
        /// </summary>
        /// <param name="id">Identificador da conquista.</param>
        /// <param name="request">Novos dados da conquista.</param>
        /// <returns>Conquista atualizada.</returns>
        public async Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request)
        {
            var achievement = EnsureFound(await _context.BonusAchievements.FirstOrDefaultAsync(x => x.Id == id));

            EnsureUnique(await _context.BonusAchievements.AnyAsync(x => x.Code == request.Code && x.Id != id), "Code", $"Já existe outra conquista com o código '{request.Code}'.");

            achievement.Code = request.Code;
            achievement.Name = request.Name;
            achievement.Description = request.Description;
            achievement.PointsRequired = request.PointsRequired;
            achievement.DisplayType = (AchievementDisplayType)request.DisplayType;
            achievement.IsActive = request.IsActive;

            await SaveChangesContextAsync();

            return await GetAchievementAsync(achievement.Id);
        }

        /// <summary>
        /// Remove uma conquista de bonus.
        /// </summary>
        /// <param name="id">Identificador da conquista.</param>
        public async Task DeleteAchievementAsync(Guid id)
        {
            var achievement = EnsureFound(await _context.BonusAchievements.FirstOrDefaultAsync(x => x.Id == id));
            
            try
            {
                _context.BonusAchievements.Remove(achievement);
                await SaveChangesContextAsync();
            }
            catch (Exception)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Não é possível excluir esta conquista pois ela já está vinculada a usuários.");
            }
        }

        #endregion

        #region Bonus Point Rules

        /// <summary>
        /// Retorna a lista de regras de pontuacao de bonus.
        /// </summary>
        /// <returns>Lista de regras de bonus.</returns>
        public async Task<List<BonusPointRuleDTO>> GetRulesAsync()
        {
            return await _context.BonusPointRules
                .OrderBy(r => (int)r.Source)
                .Select(r => new BonusPointRuleDTO
                {
                    Id = r.Id,
                    Source = (int)r.Source,
                    Name = r.Name,
                    Description = r.Description,
                    DefaultPoints = r.DefaultPoints,
                    AllowCustomPoints = r.AllowCustomPoints,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma regra de pontuacao de bonus pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da regra.</param>
        /// <returns>Dados da regra de bonus.</returns>
        public async Task<BonusPointRuleDTO> GetRuleAsync(Guid id)
        {
            var r = EnsureFound(await _context.BonusPointRules.FirstOrDefaultAsync(x => x.Id == id));
            return new BonusPointRuleDTO
            {
                Id = r.Id,
                Source = (int)r.Source,
                Name = r.Name,
                Description = r.Description,
                DefaultPoints = r.DefaultPoints,
                AllowCustomPoints = r.AllowCustomPoints,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            };
        }

        /// <summary>
        /// Cria uma nova regra de pontuacao de bonus.
        /// </summary>
        /// <param name="request">Dados da regra.</param>
        /// <returns>Regra criada.</returns>
        public async Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request)
        {
            EnsureUnique(await _context.BonusPointRules.AnyAsync(x => (int)x.Source == request.Source), "Source", "Já existe uma regra para esta origem de pontos.");

            var rule = new BonusPointRule
            {
                Source = (BonusPointSource)request.Source,
                Name = request.Name,
                Description = request.Description,
                DefaultPoints = request.DefaultPoints,
                AllowCustomPoints = request.AllowCustomPoints,
                IsActive = request.IsActive
            };

            await _context.BonusPointRules.AddAsync(rule);
            await SaveChangesContextAsync();

            return await GetRuleAsync(rule.Id);
        }

        /// <summary>
        /// Atualiza uma regra de pontuacao de bonus existente.
        /// </summary>
        /// <param name="id">Identificador da regra.</param>
        /// <param name="request">Novos dados da regra.</param>
        /// <returns>Regra atualizada.</returns>
        public async Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request)
        {
            var rule = EnsureFound(await _context.BonusPointRules.FirstOrDefaultAsync(x => x.Id == id));

            rule.Name = request.Name;
            rule.Description = request.Description;
            rule.DefaultPoints = request.DefaultPoints;
            rule.AllowCustomPoints = request.AllowCustomPoints;
            rule.IsActive = request.IsActive;
            rule.UpdatedAt = Models.Utils.SaoPauloDateTime.Now();

            await SaveChangesContextAsync();

            return await GetRuleAsync(rule.Id);
        }

        /// <summary>
        /// Remove uma regra de pontuacao de bonus.
        /// </summary>
        /// <param name="id">Identificador da regra.</param>
        public async Task DeleteRuleAsync(Guid id)
        {
            var rule = EnsureFound(await _context.BonusPointRules.FirstOrDefaultAsync(x => x.Id == id));
            _context.BonusPointRules.Remove(rule);
            await SaveChangesContextAsync();
        }

        #endregion
    }
}
