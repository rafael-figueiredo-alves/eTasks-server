namespace eTasks_server.Models.DTOs.Auth.Responses
{
    public class GoogleAuthStatusResponse
    {
        public Guid SessionCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
