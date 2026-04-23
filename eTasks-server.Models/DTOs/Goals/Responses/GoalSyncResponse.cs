namespace eTasks_server.Models.DTOs.Goals.Responses
{
    public class GoalSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<GoalDetailsResponse> Upserts { get; set; } = [];
        public List<DeletedGoalResponse> Deleted { get; set; } = [];
    }
}
