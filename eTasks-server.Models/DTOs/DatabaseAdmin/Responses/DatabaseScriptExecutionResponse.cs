namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    public class DatabaseScriptExecutionResponse
    {
        public bool Success { get; set; }
        public int AffectedRows { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
    }
}
