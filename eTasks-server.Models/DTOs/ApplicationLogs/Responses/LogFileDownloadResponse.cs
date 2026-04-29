namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    public class LogFileDownloadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/plain";
        public byte[] Content { get; set; } = [];
    }
}
