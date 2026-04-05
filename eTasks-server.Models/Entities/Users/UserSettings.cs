using eTasks_server.Models.Utils;

namespace eTasks_server.Models.Entities.Users
{
    public class UserSettings
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "pt";
        public bool UseCamera { get; set; }
        public bool EnableBonusSystem { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }
    }
}
