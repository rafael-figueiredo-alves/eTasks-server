using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resposta detalhada de uma tarefa.
    /// </summary>
    public class TaskDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid UserUid { get; set; }
        public Guid? GeneratedFromTaskId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime TaskDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public TaskRecurrenceResponse? Recurrence { get; set; }
    }
}
