namespace eTasks_server.Models.DTOs.Finances.Requests
{
    public class FinanceEntryPushSyncItemRequest
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public FinanceEntryPushOperationType Operation { get; set; }
        public Guid? FinanceEntryId { get; set; }
        public string? ExpectedEtag { get; set; }
        public CreateFinanceEntryRequest? Create { get; set; }
        public UpdateFinanceEntryRequest? Update { get; set; }
    }
}
