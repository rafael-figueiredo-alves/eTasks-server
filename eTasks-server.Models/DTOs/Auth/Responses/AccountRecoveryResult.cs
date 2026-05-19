namespace eTasks_server.Models.DTOs.Auth.Responses
{
    public class AccountRecoveryResult
    {
        public bool Success { get; set; }
        public bool Expired { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
