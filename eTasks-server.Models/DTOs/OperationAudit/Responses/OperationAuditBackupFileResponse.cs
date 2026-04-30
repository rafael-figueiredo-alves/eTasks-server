namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    public class OperationAuditBackupFileResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/x-ndjson";
        public byte[] Content { get; set; } = [];
    }
}
