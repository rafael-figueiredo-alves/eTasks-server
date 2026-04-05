namespace eTasks_server.Models.DTOs.Auth.Responses
{
    public class PasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
