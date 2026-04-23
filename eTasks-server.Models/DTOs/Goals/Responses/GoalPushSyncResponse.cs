namespace eTasks_server.Models.DTOs.Goals.Responses
{
    public class GoalPushSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<GoalPushSyncItemResponse> Results { get; set; } = [];
    }
}
