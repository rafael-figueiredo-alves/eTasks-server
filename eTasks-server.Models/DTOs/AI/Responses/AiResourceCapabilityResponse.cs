using eTasks_server.Models.DTOs.AI.Requests;

namespace eTasks_server.Models.DTOs.AI.Responses
{
    public class AiResourceCapabilityResponse
    {
        public AiResourceType Resource { get; set; }
        public string Label { get; set; } = string.Empty;
        public List<string> RecommendedUses { get; set; } = [];
        public List<string> SupportedIntents { get; set; } = [];
        public List<string> Guardrails { get; set; } = [];
        public AiPayloadTemplateResponse PayloadTemplate { get; set; } = new();
    }
}
