namespace eTasks_server.Models.DTOs.Goals.Requests
{
    public class GoalPushSyncRequest
    {
        public List<GoalPushSyncItemRequest> Operations { get; set; } = [];
    }
}
