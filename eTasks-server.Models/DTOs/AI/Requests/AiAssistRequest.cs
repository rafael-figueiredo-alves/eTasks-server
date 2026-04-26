namespace eTasks_server.Models.DTOs.AI.Requests
{
    public class AiAssistRequest
    {
        public AiResourceType Resource { get; set; } = AiResourceType.General;
        public AiInteractionIntent Intent { get; set; } = AiInteractionIntent.GeneralHelp;
        public string UserPrompt { get; set; } = string.Empty;
        public string? ResourceTitle { get; set; }
        public string? ResourceContent { get; set; }
        public string? AdditionalContext { get; set; }
        public List<AiConversationMessageRequest> Conversation { get; set; } = [];
    }
}
