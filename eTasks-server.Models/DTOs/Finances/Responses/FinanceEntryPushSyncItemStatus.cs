namespace eTasks_server.Models.DTOs.Finances.Responses
{
    public enum FinanceEntryPushSyncItemStatus
    {
        Applied = 0,
        Conflict = 1,
        ValidationError = 2,
        NotFound = 3,
        Failed = 4
    }
}
