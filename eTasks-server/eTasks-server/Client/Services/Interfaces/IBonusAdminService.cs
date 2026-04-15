using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IBonusAdminService
    {
        // Bonus Achievements
        Task<List<BonusAchievementDTO>> GetAchievementsAsync();
        Task<BonusAchievementDTO> GetAchievementAsync(Guid id);
        Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request);
        Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request);
        Task DeleteAchievementAsync(Guid id);

        // Bonus Point Rules
        Task<List<BonusPointRuleDTO>> GetRulesAsync();
        Task<BonusPointRuleDTO> GetRuleAsync(Guid id);
        Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request);
        Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request);
        Task DeleteRuleAsync(Guid id);
    }
}
