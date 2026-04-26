namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    public enum ShoppingListPushSyncItemStatus
    {
        Applied = 0,
        Conflict = 1,
        ValidationError = 2,
        NotFound = 3,
        Failed = 4
    }
}
