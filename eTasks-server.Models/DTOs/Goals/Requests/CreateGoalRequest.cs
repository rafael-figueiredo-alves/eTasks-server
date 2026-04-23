using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Dados para criar uma meta.
    /// </summary>
    public class CreateGoalRequest
    {
        public Guid? ClientGeneratedId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? Description { get; set; }
        public GoalType Type { get; set; } = GoalType.Personal;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int? RewardPoints { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.Active;
    }
}
