using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;

namespace eTasks_server.Client.Services
{
    public class BonusAdminService : IBonusAdminService
    {
        private readonly IBonusAdminBLL _bll;

        public BonusAdminService(IBonusAdminBLL bll)
        {
            _bll = bll;
        }

        #region Bonus Achievements

        public async Task<List<BonusAchievementDTO>> GetAchievementsAsync()
        {
            return await _bll.GetAchievementsAsync();
        }

        public async Task<BonusAchievementDTO> GetAchievementAsync(Guid id)
        {
            return await _bll.GetAchievementAsync(id);
        }

        public async Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request)
        {
            return await _bll.CreateAchievementAsync(request);
        }

        public async Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request)
        {
            return await _bll.UpdateAchievementAsync(id, request);
        }

        public async Task DeleteAchievementAsync(Guid id)
        {
            await _bll.DeleteAchievementAsync(id);
        }

        #endregion

        #region Bonus Point Rules

        public async Task<List<BonusPointRuleDTO>> GetRulesAsync()
        {
            return await _bll.GetRulesAsync();
        }

        public async Task<BonusPointRuleDTO> GetRuleAsync(Guid id)
        {
            return await _bll.GetRuleAsync(id);
        }

        public async Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request)
        {
            return await _bll.CreateRuleAsync(request);
        }

        public async Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request)
        {
            return await _bll.UpdateRuleAsync(id, request);
        }

        public async Task DeleteRuleAsync(Guid id)
        {
            await _bll.DeleteRuleAsync(id);
        }

        #endregion
    }
}
