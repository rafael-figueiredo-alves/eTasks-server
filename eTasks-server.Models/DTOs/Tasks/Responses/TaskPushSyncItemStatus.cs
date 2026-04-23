namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Status de processamento de uma mutacao no push sync.
    /// </summary>
    public enum TaskPushSyncItemStatus
    {
        Applied = 1,
        Conflict = 2,
        ValidationError = 3,
        NotFound = 4,
        Failed = 5
    }
}
