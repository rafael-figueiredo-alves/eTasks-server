using eTasks_server.Models.Enums.Tasks;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados para atualizar uma tarefa.
    /// </summary>
    public class UpdateTaskRequest
    {
        /// <summary>
        /// Resumo ou título da tarefa. Deve ser uma string não vazia.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Anotações ou detalhes adicionais sobre a tarefa. Pode ser uma string vazia ou nula.
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Prioridade da tarefa. O valor padrão é Medium.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        /// <summary>
        /// Data da tarefa.
        /// </summary>
        public DateTime TaskDate { get; set; }
        
        /// <summary>
        /// Indica se a tarefa foi concluída.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Dados de recorrência da tarefa. Pode ser nulo se a tarefa não for recorrente.
        /// </summary>
        public TaskRecurrenceRequest? Recurrence { get; set; }
    }
}
