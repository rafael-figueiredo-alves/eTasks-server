using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Item resumido da listagem de tarefas.
    /// </summary>
    public class TaskListItemResponse
    {
        public Guid Id { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime TaskDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool HasRecurrence { get; set; }
    }
}
