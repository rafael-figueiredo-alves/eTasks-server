namespace eTasks_server.Models.DTOs.Finances.Responses
{
    public class FinanceEntryPushSyncItemResponse
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public FinanceEntryPushSyncItemStatus Status { get; set; }
        public FinanceEntryDetailsResponse? FinanceEntry { get; set; }
        public DeletedFinanceEntryResponse? Deleted { get; set; }
        public string? ServerEtag { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
