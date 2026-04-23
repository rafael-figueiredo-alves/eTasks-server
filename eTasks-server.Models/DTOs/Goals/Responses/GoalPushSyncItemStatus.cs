namespace eTasks_server.Models.DTOs.Goals.Responses
{
    public enum GoalPushSyncItemStatus
    {
        Applied = 1,
        Conflict = 2,
        ValidationError = 3,
        NotFound = 4,
        Failed = 5
    }
}
