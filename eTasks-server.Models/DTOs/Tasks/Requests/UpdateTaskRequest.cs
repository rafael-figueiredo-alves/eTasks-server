using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados para atualizar uma tarefa.
    /// </summary>
    public class UpdateTaskRequest
    {
        public string Summary { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime TaskDate { get; set; }
        public bool IsCompleted { get; set; }
        public TaskRecurrenceRequest? Recurrence { get; set; }
    }
}
