namespace eTasks_server.Models.DTOs.OperationAudit.Requests
{
    public class OperationAuditLogQueryRequest
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; } = 25;
        public string Search { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public string ResourceName { get; set; } = string.Empty;
    }
}
