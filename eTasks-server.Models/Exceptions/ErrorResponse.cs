using System.Text.Json.Serialization;

namespace eTasks_server.Models.Exceptions
{
    public class ErrorResponse
    {
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public string Details { get; set; } = string.Empty;
    }
}
