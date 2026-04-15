namespace eTasks_server.Models.DTOs.Gamification.BonusAchievement.Responses
{
    /// <summary>
    /// DTO de resposta que representa os dados de uma conquista de bônus,
    /// utilizado nas listagens e detalhes expostos pelo painel administrativo.
    /// </summary>
    public class BonusAchievementDTO
    {
        /// <summary>
        /// Identificador único da conquista.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Código único da conquista, utilizado como referência interna e nos clientes.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nome/título amigável da conquista.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição opcional com detalhes sobre os critérios da conquista.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Quantidade de pontos necessária para que o usuário alcance esta conquista.
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// Tipo de exibição visual da conquista (0 = Troféu, 1 = Medalha).
        /// </summary>
        public int DisplayType { get; set; }

        /// <summary>
        /// Indica se a conquista está ativa e disponível para ser conquistada pelos usuários.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Data de criação do registro.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
