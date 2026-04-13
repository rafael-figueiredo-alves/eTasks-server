using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Filtros de consulta para a listagem de tarefas.
    /// </summary>
    public class ListTasksRequest
    {
        public DateTime? ReferenceDate { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? IsCompleted { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? SearchTerm { get; set; }
    }
}
