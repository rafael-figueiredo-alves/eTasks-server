using eTasks_server.Models.Entities.Users;

namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserSettingsSyncResponse
    {
        public Guid Id { get; set; }
        public Guid UserUid { get; set; }
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "pt-BR";
        public AppStartScreen InitialScreen { get; set; } = AppStartScreen.Home;
        public bool EnableBonusSystem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
