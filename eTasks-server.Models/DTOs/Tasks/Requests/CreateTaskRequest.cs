using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados para criar uma tarefa.
    /// </summary>
    public class CreateTaskRequest
    {
        /// <summary>
        /// Identificador opcional gerado pelo cliente para cenarios offline-first.
        /// </summary>
        public Guid? ClientGeneratedId { get; set; }

        /// <summary>
        /// Resumo ou titulo da tarefa.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Anotacoes ou detalhes adicionais sobre a tarefa.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Prioridade da tarefa.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        /// <summary>
        /// Data da tarefa.
        /// </summary>
        public DateTime TaskDate { get; set; }

        /// <summary>
        /// Indica se a tarefa foi concluida.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Recorrencia da tarefa.
        /// </summary>
        public TaskRecurrenceRequest? Recurrence { get; set; }
    }
}
