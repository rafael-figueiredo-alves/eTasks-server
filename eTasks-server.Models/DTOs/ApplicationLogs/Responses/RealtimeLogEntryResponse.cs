namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    public class RealtimeLogEntryResponse
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
