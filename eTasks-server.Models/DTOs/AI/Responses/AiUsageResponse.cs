namespace eTasks_server.Models.DTOs.AI.Responses
{
    public class AiUsageResponse
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
