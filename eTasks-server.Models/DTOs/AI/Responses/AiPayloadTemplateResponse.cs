namespace eTasks_server.Models.DTOs.AI.Responses
{
    public class AiPayloadTemplateResponse
    {
        public string RoutePattern { get; set; } = string.Empty;
        public string Method { get; set; } = "POST";
        public string SuggestedResourceTitlePattern { get; set; } = string.Empty;
        public string SuggestedResourceContentPattern { get; set; } = string.Empty;
        public string SuggestedAdditionalContextPattern { get; set; } = string.Empty;
        public List<AiPayloadFieldResponse> Fields { get; set; } = [];
        public List<string> ExamplePrompts { get; set; } = [];
    }
}
