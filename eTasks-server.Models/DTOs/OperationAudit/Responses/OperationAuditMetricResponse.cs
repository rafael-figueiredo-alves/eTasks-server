namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    public class OperationAuditMetricResponse
    {
        public string Label { get; set; } = string.Empty;
        public long Count { get; set; }
    }
}
