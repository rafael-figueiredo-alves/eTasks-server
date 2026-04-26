namespace eTasks_server.Models.DTOs.Finances.Requests
{
    public class FinanceEntryPushSyncRequest
    {
        public List<FinanceEntryPushSyncItemRequest> Operations { get; set; } = [];
    }
}
