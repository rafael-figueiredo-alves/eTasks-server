namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    public class LogFileContentResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
