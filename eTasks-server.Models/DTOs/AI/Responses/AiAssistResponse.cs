namespace eTasks_server.Models.DTOs.AI.Responses
{
    public class AiAssistResponse
    {
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public AiUsageResponse Usage { get; set; } = new();
    }
}
