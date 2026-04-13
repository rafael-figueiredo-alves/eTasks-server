using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Filtros de consulta para metas.
    /// </summary>
    public class ListGoalsRequest
    {
        public GoalStatus? Status { get; set; }
        public GoalType? Type { get; set; }
        public TaskPriority? Priority { get; set; }
        public bool? OnlyRewarded { get; set; }
        public string? SearchTerm { get; set; }
    }
}
