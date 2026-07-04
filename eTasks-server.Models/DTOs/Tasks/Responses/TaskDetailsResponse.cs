using eTasks_server.Models.Enums.Tasks;

namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resposta detalhada de uma tarefa.
    /// </summary>
    public class TaskDetailsResponse
    {
        /// <summary>
        /// Identificador único da tarefa.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Identificador único do usuário associado à tarefa.
        /// </summary>
        public Guid UserUid { get; set; }
        
        /// <summary>
        /// Identificador da tarefa da qual esta tarefa foi gerada, se aplicável.
        /// </summary>
        public Guid? GeneratedFromTaskId { get; set; }
        
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
        /// Data e hora em que a tarefa foi criada.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Data e hora da última atualização da tarefa, se aplicável.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Dados de recorrência da tarefa, se aplicável.
        /// </summary>
        public TaskRecurrenceResponse? Recurrence { get; set; }
    }
}
