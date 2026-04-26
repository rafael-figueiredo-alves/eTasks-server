namespace eTasks_server.Models.DTOs.Readings.Responses
{
    public enum ReadingPushSyncItemStatus
    {
        Applied = 0,
        Conflict = 1,
        ValidationError = 2,
        NotFound = 3,
        Failed = 4
    }
}
