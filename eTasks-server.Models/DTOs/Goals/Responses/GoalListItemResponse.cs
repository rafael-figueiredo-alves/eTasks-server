using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Goals.Responses
{
    /// <summary>
    /// Item resumido da listagem de metas.
    /// </summary>
    public class GoalListItemResponse
    {
        /// <summary>
        /// Identificador único da meta.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Resumo ou título da meta.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da meta (opcional).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Tipo da meta.
        /// </summary>
        public GoalType Type { get; set; }

        /// <summary>
        /// Prioridade da meta.
        /// </summary>
        public TaskPriority Priority { get; set; }

        /// <summary>
        /// Pontos de recompensa associados à meta.
        /// </summary>
        public int? RewardPoints { get; set; }

        /// <summary>
        /// Status da meta.
        /// </summary>
        public GoalStatus Status { get; set; }

        /// <summary>
        /// Data e hora de criação da meta.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data e hora da última atualização da meta (opcional).
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
