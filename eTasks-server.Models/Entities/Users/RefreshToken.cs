using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;

namespace eTasks_server.Models.Entities.Users
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public string Token { get; set; } = string.Empty;

        [AllowedUserAgent]
        public string? UserAgent { get; set; }

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }
    }
}
