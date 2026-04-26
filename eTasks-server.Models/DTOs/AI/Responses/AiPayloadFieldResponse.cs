namespace eTasks_server.Models.DTOs.AI.Responses
{
    public class AiPayloadFieldResponse
    {
        public string Name { get; set; } = string.Empty;
        public string TargetProperty { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Required { get; set; }
    }
}
