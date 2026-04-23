namespace eTasks_server.Models.DTOs.Goals.Responses
{
    public class GoalPushSyncItemResponse
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public GoalPushSyncItemStatus Status { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public GoalDetailsResponse? Goal { get; set; }
        public DeletedGoalResponse? Deleted { get; set; }
        public string? ServerEtag { get; set; }
    }
}
