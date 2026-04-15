namespace eTasks_server.Models.DTOs.Gamification.BonusPointRule.Responses
{
    /// <summary>
    /// DTO de resposta que representa os dados de uma regra de bônus,
    /// utilizado nas listagens e detalhes expostos pelo painel administrativo.
    /// </summary>
    public class BonusPointRuleDTO
    {
        /// <summary>
        /// Identificador único da regra.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Origem do ponto coberta pela regra (valor inteiro do enum BonusPointSource).
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// Nome amigável da regra.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição resumida da finalidade da regra.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Quantidade padrão de pontos atribuída quando essa origem ocorre.
        /// </summary>
        public int DefaultPoints { get; set; }

        /// <summary>
        /// Indica se a pontuação pode ser sobrescrita em lançamentos específicos.
        /// </summary>
        public bool AllowCustomPoints { get; set; }

        /// <summary>
        /// Indica se a regra está ativa e sendo aplicada.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Data de criação do registro.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização da regra, se houver.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
