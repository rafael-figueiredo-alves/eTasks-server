using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço de administração de bônus, responsável por gerenciar conquistas de bônus e regras de pontos de bônus dentro do sistema de gamificação. Essa interface define os métodos necessários para criar, ler, atualizar e excluir conquistas de bônus e regras de pontos de bônus, permitindo que os administradores possam configurar e personalizar as recompensas oferecidas aos usuários, incentivando o engajamento e a participação ativa na plataforma. A implementação dessa interface é fundamental para garantir uma gestão eficiente das funcionalidades de gamificação relacionadas a bônus, promovendo uma experiência mais envolvente e motivadora para os usuários.
    /// </summary>
    public interface IBonusAdminService
    {
        #region Bonus Achievements
        /// <summary>
        /// Obtem a lista de conquistas de bônus, incluindo detalhes como código, nome, descrição, pontos necessários, tipo de exibição e status ativo. Essa funcionalidade é essencial para que os administradores possam gerenciar e visualizar as conquistas disponíveis para os usuários, permitindo uma melhor organização e incentivo dentro do sistema de gamificação.
        /// </summary>
        /// <returns></returns>
        Task<List<BonusAchievementDTO>> GetAchievementsAsync();

        /// <summary>
        /// Obtem os detalhes de uma conquista de bônus específica com base no seu ID. Essa funcionalidade é crucial para que os administradores possam acessar informações detalhadas sobre uma conquista específica, como código, nome, descrição, pontos necessários, tipo de exibição e status ativo, permitindo uma gestão mais eficiente das conquistas dentro do sistema de gamificação.
        /// </summary>
        /// <param name="id">ID da conquista de bônus</param>
        /// <returns>Detalhes da conquista de bônus</returns>
        Task<BonusAchievementDTO> GetAchievementAsync(Guid id);

        /// <summary>
        /// Cria uma nova conquista de bônus com base nas informações fornecidas na requisição. Essa funcionalidade é fundamental para que os administradores possam adicionar novas conquistas ao sistema de gamificação, incentivando os usuários a alcançarem novos objetivos e aumentando o engajamento dentro da plataforma. A criação de conquistas de bônus permite uma personalização e diversificação das recompensas oferecidas aos usuários, promovendo uma experiência mais envolvente e motivadora.
        /// </summary>
        /// <param name="request">Dados da nova conquista de bônus</param>
        /// <returns>Detalhes da conquista de bônus criada</returns>
        Task<BonusAchievementDTO> CreateAchievementAsync(BonusAchievementRequest request);

        /// <summary>
        /// Atualiza uma conquista de bônus existente com base no seu ID e nas informações fornecidas na requisição. Essa funcionalidade é essencial para que os administradores possam modificar as conquistas de bônus existentes, permitindo ajustes e melhorias conforme necessário. A atualização de conquistas de bônus possibilita a adaptação das recompensas oferecidas aos usuários, garantindo que elas permaneçam relevantes e motivadoras ao longo do tempo, contribuindo para um sistema de gamificação mais dinâmico e eficaz.
        /// </summary>
        /// <param name="id">ID da conquista de bônus</param>
        /// <param name="request">Dados da atualização da conquista de bônus</param>
        /// <returns>Detalhes da conquista de bônus atualizada</returns>
        Task<BonusAchievementDTO> UpdateAchievementAsync(Guid id, BonusAchievementRequest request);
        
        /// <summary>
        /// Exclui uma conquista de bônus existente com base no seu ID. Essa funcionalidade é essencial para que os administradores possam remover conquistas de bônus que não são mais relevantes ou necessárias, garantindo que o sistema de gamificação permaneça atualizado e alinhado com os objetivos da plataforma.
        /// </summary>
        /// <param name="id">ID da conquista de bônus</param>
        Task DeleteAchievementAsync(Guid id);
        #endregion

        #region Bonus Point Rules
        /// <summary>
        /// Obtém a lista de regras de pontos de bônus, incluindo detalhes como código, nome, descrição, pontos padrão, permissão para pontos personalizados e status ativo. Essa funcionalidade é essencial para que os administradores possam gerenciar e visualizar as regras de pontos de bônus disponíveis para os usuários, permitindo uma melhor organização e incentivo dentro do sistema de gamificação.
        /// </summary>
        /// <returns></returns>
        Task<List<BonusPointRuleDTO>> GetRulesAsync();

        /// <summary>
        /// Obtem os detalhes de uma regra de pontos de bônus específica com base no seu ID. Essa funcionalidade é crucial para que os administradores possam acessar informações detalhadas sobre uma regra de pontos de bônus específica, como código, nome, descrição, pontos padrão, permissão para pontos personalizados e status ativo, permitindo uma gestão mais eficiente das regras de pontos de bônus dentro do sistema de gamificação.
        /// </summary>
        /// <param name="id">ID da regra de pontos de bônus</param>
        /// <returns>Detalhes da regra de pontos de bônus</returns>
        Task<BonusPointRuleDTO> GetRuleAsync(Guid id);

        /// <summary>
        /// Cria uma nova regra de pontos de bônus com base nas informações fornecidas na requisição. Essa funcionalidade é fundamental para que os administradores possam adicionar novas regras de pontos de bônus ao sistema de gamificação, incentivando os usuários a alcançarem novos objetivos e aumentando o engajamento dentro da plataforma. A criação de regras de pontos de bônus permite uma personalização e diversificação das recompensas oferecidas aos usuários, promovendo uma experiência mais envolvente e motivadora.
        /// </summary>
        /// <param name="request">Dados da nova regra de pontos de bônus</param>
        /// <returns>Detalhes da regra de pontos de bônus criada</returns>
        Task<BonusPointRuleDTO> CreateRuleAsync(BonusPointRuleCreateRequest request);

        /// <summary>
        /// Atualiza uma regra de pontos de bônus existente com base no seu ID e nas informações fornecidas na requisição. Essa funcionalidade é essencial para que os administradores possam modificar as regras de pontos de bônus existentes, permitindo ajustes e melhorias conforme necessário. A atualização de regras de pontos de bônus possibilita a adaptação das recompensas oferecidas aos usuários, garantindo que elas permaneçam relevantes e motivadoras ao longo do tempo, contribuindo para um sistema de gamificação mais dinâmico e eficaz.
        /// </summary>
        /// <param name="id">ID da regra de pontos de bônus</param>
        /// <param name="request">Dados da atualização da regra de pontos de bônus</param>
        /// <returns>Detalhes da regra de pontos de bônus atualizada</returns>
        Task<BonusPointRuleDTO> UpdateRuleAsync(Guid id, BonusPointRuleUpdateRequest request);

        /// <summary>
        /// Remove uma regra de pontos de bônus existente com base no seu ID. Essa funcionalidade é essencial para que os administradores possam excluir regras de pontos de bônus que não são mais relevantes ou necessárias, garantindo que o sistema de gamificação permaneça atualizado e alinhado com os objetivos da plataforma.
        /// </summary>
        /// <param name="id">ID da regra de pontos de bônus</param>
        /// <returns></returns>
        Task DeleteRuleAsync(Guid id);
        #endregion
    }
}
