namespace eTasks_server.Models.DTOs.Auth.Responses
{
    public class GoogleAuthStartResponse
    {
        public Guid SessionCode { get; set; }
        public string AuthorizationUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
