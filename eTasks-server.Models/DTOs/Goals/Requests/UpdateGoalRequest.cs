using eTasks_server.Models.Enums.Goals;
using eTasks_server.Models.Enums.Tasks;

namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Dados para atualizar uma meta.
    /// </summary>
    public class UpdateGoalRequest
    {
        /// <summary>
        /// Resumo da meta. Deve ser uma descrição breve e clara do objetivo a ser alcançado.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da meta. Pode incluir informações adicionais, etapas para alcançar o objetivo, prazos, etc. Deve fornecer um contexto mais completo sobre a meta e o que é necessário para alcançá-la.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Tipo da meta, que pode ser pessoal, profissional, de saúde, etc. Isso ajuda a categorizar a meta e pode influenciar como ela é gerenciada e acompanhada.
        /// </summary>
        public GoalType Type { get; set; } = GoalType.Personal;

        /// <summary>
        /// Prioridade da meta, que pode ser baixa, média ou alta. Isso ajuda a determinar a importância da meta em relação a outras metas e pode influenciar a alocação de tempo e recursos para alcançá-la.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <summary>
        /// Pontos de recompensa associados à meta. Se a meta for recompensada, este campo deve conter o número de pontos que serão concedidos ao usuário ao alcançar a meta. Se a meta não for recompensada, este campo deve ser nulo ou zero.
        /// </summary>
        public int? RewardPoints { get; set; }

        /// <summary>
        /// Status da meta, que pode ser ativo, concluído, arquivado, etc. O status ajuda a acompanhar o progresso da meta e pode influenciar como ela é exibida e gerenciada no sistema.
        /// </summary>
        public GoalStatus Status { get; set; } = GoalStatus.Active;
    }
}
