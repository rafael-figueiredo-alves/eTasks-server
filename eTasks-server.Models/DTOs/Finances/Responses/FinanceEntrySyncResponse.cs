namespace eTasks_server.Models.DTOs.Finances.Responses
{
    public class FinanceEntrySyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<FinanceEntryDetailsResponse> Upserts { get; set; } = [];
        public List<DeletedFinanceEntryResponse> Deleted { get; set; } = [];
    }
}
