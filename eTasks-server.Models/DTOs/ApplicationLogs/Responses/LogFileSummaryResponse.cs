namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    public class LogFileSummaryResponse
    {
        public string FileName { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
}
