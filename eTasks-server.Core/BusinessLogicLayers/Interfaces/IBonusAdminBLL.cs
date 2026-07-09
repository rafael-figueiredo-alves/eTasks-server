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
        #region Bonus Achievements (Conquistas)
        /// <summary>
        /// Obtem conquistas
        /// </summary>
        /// <returns></returns>
        Task<List<BonusAchievementDTO>> GetAchievementsAsync();

        /// <summary>
        /// Obtem uma conquista em específico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BonusAchievementDTO> GetAchievementAsync(Guid id);

        /// <summary>
        /// Cria uma nova conquista
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request);

        /// <summary>
        /// Atualiza uma conquista
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request);

        /// <summary>
        /// Apaga uma conquista
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAchievementAsync(Guid id);
        #endregion

        #region Bonus Point Rules (Regras)
        /// <summary>
        /// Obtem regras para obter pontos para conquistas
        /// </summary>
        /// <returns></returns>
        Task<List<BonusPointRuleDTO>> GetRulesAsync();

        /// <summary>
        /// Obtem regra especifica
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<BonusPointRuleDTO> GetRuleAsync(Guid id);

        /// <summary>
        /// Cria uma nova regra
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request);

        /// <summary>
        /// Atualiza uma regra
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request);

        /// <summary>
        /// Exclui uma regra
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteRuleAsync(Guid id);
        #endregion
    }
}
