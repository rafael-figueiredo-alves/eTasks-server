using System.Text.Json.Serialization;

namespace eTasks_server.Models.Exceptions
{
    public class ErrorResponse
    {
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("Errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ErrorDetail>? Errors { get; set; }
    }

    public class ErrorDetail
    {
        [JsonPropertyName("Campo")]
        public string Campo { get; set; } = string.Empty;

        [JsonPropertyName("Erro")]
        public string Erro { get; set; } = string.Empty;
    }
}
