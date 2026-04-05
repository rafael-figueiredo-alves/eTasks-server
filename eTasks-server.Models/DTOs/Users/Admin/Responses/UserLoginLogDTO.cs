namespace eTasks_server.Models.DTOs.Users.Admin.Responses
{
    public class UserLoginLogDTO
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
