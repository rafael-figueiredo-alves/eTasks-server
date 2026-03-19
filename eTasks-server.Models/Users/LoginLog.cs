using System;
using System.Text.Json.Serialization;

namespace eTasks_server.Models.Users
{
    public class LoginLog
    {
        public Guid Id { get; set; } = Guid.CreateVersion7(); // Requer .NET 10 ou custom
        public Guid? UserUid { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., "Success", "Failed"
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public User? User { get; set; }
    }
}
