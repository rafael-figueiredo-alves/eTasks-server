using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Item resumido da listagem de tarefas.
    /// </summary>
    public class TaskListItemResponse
    {
        /// <summary>
        /// Identificador único da tarefa.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Resumo ou título da tarefa.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
        
        /// <summary>
        /// Anotações ou detalhes adicionais sobre a tarefa.
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Prioridade da tarefa.
        /// </summary>
        public TaskPriority Priority { get; set; }
        
        /// <summary>
        /// Data da tarefa.
        /// </summary>
        public DateTime TaskDate { get; set; }
        
        /// <summary>
        /// Indica se a tarefa foi concluída.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Data e hora em que a tarefa foi concluída, se aplicável.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Indica se a tarefa possui recorrência.
        /// </summary>
        public bool HasRecurrence { get; set; }
    }
}
