namespace eTasks_server.Models.DTOs.Users.Profile.Requests
{
    public class PatchUserSettingsRequest
    {
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public bool? UseCamera { get; set; }
        public bool? EnableBonusSystem { get; set; }
    }
}
