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
        /// Resumo ou título da meta. Deve ser breve e descritivo.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da meta. Pode incluir informações adicionais, critérios de sucesso ou etapas para alcançá-la.
        /// </summary>
        public string? Description { get; set; }


        /// <summary>
        /// Tipo de meta, como pessoal, profissional, saúde, etc. Isso pode ajudar a categorizar e organizar as metas do usuário.
        /// </summary>
        public GoalType Type { get; set; } = GoalType.Personal;

        /// <summary>
        /// Prioridade da meta, que pode ser usada para ajudar o usuário a focar nas metas mais importantes. Por exemplo, alta, média ou baixa prioridade.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <summary>
        /// Pontos de recompensa que o usuário receberá ao alcançar a meta. Isso pode ser usado para incentivar o progresso e a conclusão das metas, especialmente se integrado a um sistema de gamificação.
        /// </summary>
        public int? RewardPoints { get; set; }

        /// <summary>
        /// Status inicial da meta, que pode ser ativa, inativa ou concluída. Isso permite que o usuário defina o estado da meta no momento da criação, facilitando a organização e o acompanhamento do progresso.
        /// </summary>
        public GoalStatus Status { get; set; } = GoalStatus.Active;
    }
}
