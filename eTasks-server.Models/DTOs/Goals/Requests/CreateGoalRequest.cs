using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Dados para criar uma meta.
    /// </summary>
    public class CreateGoalRequest
    {
        /// <summary>
        /// Identificador gerado pelo cliente para a meta. Se fornecido, será usado como o ID da meta; caso contrário, um novo ID será gerado.
        /// </summary>
        public Guid? ClientGeneratedId { get; set; }

        /// <summary>
        /// Resumo da meta.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da meta.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Tipo da meta (Pessoal, Profissional, etc.).
        /// </summary>
        public GoalType Type { get; set; } = GoalType.Personal;
        
        /// <summary>
        /// Prioridade da meta.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        /// <summary>
        /// Pontos de recompensa associados à meta.
        /// </summary>
        public int? RewardPoints { get; set; }
        
        /// <summary>
        /// Status da meta.
        /// </summary>
        public GoalStatus Status { get; set; } = GoalStatus.Active;
    }
}
