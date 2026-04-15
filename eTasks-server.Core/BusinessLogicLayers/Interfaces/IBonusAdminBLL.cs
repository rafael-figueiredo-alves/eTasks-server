using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de gerenciamento de Gamificação (Regras de Bônus e Conquistas) para administradores.
    /// </summary>
    public interface IBonusAdminBLL
    {
        // Bonus Achievements (Conquistas)
        Task<List<BonusAchievementDTO>> GetAchievementsAsync();
        Task<BonusAchievementDTO> GetAchievementAsync(Guid id);
        Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request);
        Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request);
        Task DeleteAchievementAsync(Guid id);

        // Bonus Point Rules (Regras)
        Task<List<BonusPointRuleDTO>> GetRulesAsync();
        Task<BonusPointRuleDTO> GetRuleAsync(Guid id);
        Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request);
        Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request);
        Task DeleteRuleAsync(Guid id);
    }
}
