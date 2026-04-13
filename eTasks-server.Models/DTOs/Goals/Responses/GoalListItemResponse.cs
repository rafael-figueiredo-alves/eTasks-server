using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Goals.Responses
{
    /// <summary>
    /// Item resumido da listagem de metas.
    /// </summary>
    public class GoalListItemResponse
    {
        public Guid Id { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? Description { get; set; }
        public GoalType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public int? RewardPoints { get; set; }
        public GoalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
