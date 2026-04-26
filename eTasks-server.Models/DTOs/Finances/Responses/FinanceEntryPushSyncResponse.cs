namespace eTasks_server.Models.DTOs.Finances.Responses
{
    public class FinanceEntryPushSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<FinanceEntryPushSyncItemResponse> Results { get; set; } = [];
    }
}
