namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserProfileResponse
    {
        public Guid Uid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAccessAt { get; set; }
        public string? PhotoBase64 { get; set; }
        public UserSettingsDTO Settings { get; set; } = new();
        public UserBonusSummaryDTO Bonus { get; set; } = new();
    }
}
