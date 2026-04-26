namespace eTasks_server.Models.DTOs.AI.Requests
{
    public class AiConversationMessageRequest
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }
}
