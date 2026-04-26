namespace eTasks_server.Models.DTOs.AI.Responses
{
    public class AiCapabilitiesResponse
    {
        public string ProviderMode { get; set; } = "OpenRouter";
        public List<string> CrossCuttingGuidance { get; set; } = [];
        public List<AiResourceCapabilityResponse> Resources { get; set; } = [];
    }
}
