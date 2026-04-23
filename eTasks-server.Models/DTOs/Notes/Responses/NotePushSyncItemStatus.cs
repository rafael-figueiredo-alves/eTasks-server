namespace eTasks_server.Models.DTOs.Notes.Responses
{
    public enum NotePushSyncItemStatus
    {
        Applied = 1,
        Conflict = 2,
        ValidationError = 3,
        NotFound = 4,
        Failed = 5
    }
}
