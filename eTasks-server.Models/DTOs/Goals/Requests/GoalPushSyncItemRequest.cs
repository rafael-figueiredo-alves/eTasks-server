namespace eTasks_server.Models.DTOs.Goals.Requests
{
    public class GoalPushSyncItemRequest
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public GoalPushOperationType Operation { get; set; }
        public Guid? GoalId { get; set; }
        public string? ExpectedEtag { get; set; }
        public CreateGoalRequest? Create { get; set; }
        public UpdateGoalRequest? Update { get; set; }
    }
}
