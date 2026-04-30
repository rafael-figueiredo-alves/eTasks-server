namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    public class OperationAuditLogEntryResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string TraceIdentifier { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? QueryString { get; set; }
        public string? EndpointName { get; set; }
        public string? ResourceName { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public Guid? UserUid { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
