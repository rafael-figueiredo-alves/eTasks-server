namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    public class OperationAuditLogPageResponse
    {
        public IReadOnlyList<OperationAuditLogEntryResponse> Items { get; set; } = [];
        public long TotalItems { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
