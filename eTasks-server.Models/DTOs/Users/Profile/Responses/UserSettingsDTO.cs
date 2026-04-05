namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserSettingsDTO
    {
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "pt";
        public bool UseCamera { get; set; }
        public bool EnableBonusSystem { get; set; }
    }
}
