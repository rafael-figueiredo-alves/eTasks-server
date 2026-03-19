namespace eTasks_server.Models.Users
{
    public class User
    {
        public Guid Uid { get; set; } = Guid.CreateVersion7();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public bool IsConfirmed { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = new List<PasswordResetCode>();
    }
}
