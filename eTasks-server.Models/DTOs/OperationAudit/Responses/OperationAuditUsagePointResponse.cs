namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    public class OperationAuditUsagePointResponse
    {
        public DateTime BucketStartUtc { get; set; }
        public string Label { get; set; } = string.Empty;
        public long TotalCount { get; set; }
        public long ErrorCount { get; set; }
    }
}
