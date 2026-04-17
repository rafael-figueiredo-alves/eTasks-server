using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;

namespace eTasks_server.Client.Services
{
    /// <summary>
    /// Serviços administrativos para o sistema de bônus, incluindo gerenciamento de conquistas e regras de pontos. Esta classe atua como uma camada intermediária entre os controladores e a lógica de negócios, facilitando a manutenção e a escalabilidade do código.
    /// </summary>
    /// <param name="_BonusAdminBLL"></param>
    public class BonusAdminService(IBonusAdminBLL _BonusAdminBLL) : IBonusAdminService
    {
        #region Bonus Achievements

        /// <summary>
        /// Traz lista de conquistas de bônus, incluindo detalhes como código, nome, descrição, pontos necessários, tipo de exibição e status de atividade. Esta funcionalidade é essencial para que os administradores possam visualizar e gerenciar as conquistas disponíveis no sistema de gamificação.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BonusAchievementDTO>> GetAchievementsAsync()
        {
            return await _BonusAdminBLL.GetAchievementsAsync();
        }

        /// <summary>
        /// Traz detalhes de uma conquista de bônus específica, identificada por seu ID. Esta funcionalidade é crucial para que os administradores possam acessar informações detalhadas sobre uma conquista específica, facilitando a edição ou análise de seu desempenho dentro do sistema de gamificação.
        /// </summary>
        /// <param name="id">Id da conquista</param>
        /// <returns></returns>
        public async Task<BonusAchievementDTO> GetAchievementAsync(Guid id)
        {
            return await _BonusAdminBLL.GetAchievementAsync(id);
        }

        /// <summary>
        /// Cria uma nova conquista de bônus com base nas informações fornecidas, como código, nome, descrição, pontos necessários, tipo de exibição e status de atividade. Esta funcionalidade é fundamental para que os administradores possam expandir o sistema de gamificação, adicionando novas conquistas que incentivem a participação e o engajamento dos usuários.
        /// </summary>
        /// <param name="request">Dados da conquista</param>
        /// <returns></returns>
        public async Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request)
        {
            return await _BonusAdminBLL.CreateAchievementAsync(request);
        }

        /// <summary>
        /// Atualiza uma conquista de bônus existente, identificada por seu ID, com base nas informações fornecidas, como código, nome, descrição, pontos necessários, tipo de exibição e status de atividade. Esta funcionalidade é essencial para que os administradores possam manter as conquistas atualizadas e relevantes, ajustando-as conforme necessário para melhor atender às necessidades dos usuários e aos objetivos do sistema de gamificação.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="request">Dados da Conquista</param>
        /// <returns></returns>
        public async Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request)
        {
            return await _BonusAdminBLL.UpdateAchievementAsync(id, request);
        }

        /// <summary>
        /// Apaga uma conquista de bônus existente, identificada por seu ID. Esta funcionalidade é crucial para que os administradores possam remover conquistas que não são mais relevantes ou que foram substituídas por outras, garantindo que o sistema de gamificação permaneça atualizado e alinhado com os objetivos da plataforma.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public async Task DeleteAchievementAsync(Guid id)
        {
            await _BonusAdminBLL.DeleteAchievementAsync(id);
        }

        #endregion

        #region Bonus Point Rules

        /// <summary>
        /// Lista regras de pontos de bônus, incluindo detalhes como código, nome, descrição, pontos padrão, permissão para pontos personalizados e status de atividade. Esta funcionalidade é essencial para que os administradores possam visualizar e gerenciar as regras de pontos disponíveis no sistema de gamificação, garantindo que elas estejam alinhadas com os objetivos da plataforma e incentivem o comportamento desejado dos usuários.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BonusPointRuleDTO>> GetRulesAsync()
        {
            return await _BonusAdminBLL.GetRulesAsync();
        }

        /// <summary>
        /// Pega detalhes de uma regra de pontos de bônus específica, identificada por seu ID. Esta funcionalidade é crucial para que os administradores possam acessar informações detalhadas sobre uma regra de pontos específica, facilitando a edição ou análise de seu desempenho dentro do sistema de gamificação.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public async Task<BonusPointRuleDTO> GetRuleAsync(Guid id)
        {
            return await _BonusAdminBLL.GetRuleAsync(id);
        }

        /// <summary>
        /// Cria uma nova regra de pontos de bônus com base nas informações fornecidas, como código, nome, descrição, pontos padrão, permissão para pontos personalizados e status de atividade. Esta funcionalidade é fundamental para que os administradores possam expandir o sistema de gamificação, adicionando novas regras de pontos que incentivem a participação e o engajamento dos usuários.
        /// </summary>
        /// <param name="request">Dados da Regra</param>
        /// <returns></returns>
        public async Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request)
        {
            return await _BonusAdminBLL.CreateRuleAsync(request);
        }

        /// <summary>
        /// Atualiza uma regra de pontos de bônus existente, identificada por seu ID, com base nas informações fornecidas, como código, nome, descrição, pontos padrão, permissão para pontos personalizados e status de atividade. Esta funcionalidade é essencial para que os administradores possam manter as regras de pontos atualizadas e relevantes, ajustando-as conforme necessário para melhor atender às necessidades dos usuários e aos objetivos do sistema de gamificação.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="request">Dados da Regra</param>
        /// <returns></returns>
        public async Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request)
        {
            return await _BonusAdminBLL.UpdateRuleAsync(id, request);
        }

        /// <summary>
        /// Remove uma regra de pontos de bônus existente, identificada por seu ID. Esta funcionalidade é crucial para que os administradores possam remover regras de pontos que não são mais relevantes ou que foram substituídas por outras, garantindo que o sistema de gamificação permaneça atualizado e alinhado com os objetivos da plataforma.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public async Task DeleteRuleAsync(Guid id)
        {
            await _BonusAdminBLL.DeleteRuleAsync(id);
        }

        #endregion
    }
}
