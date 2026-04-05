using eTasks_server.Models.Utils;

namespace eTasks_server.Models.Entities.Users
{
    public class BonusAchievement
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsRequired { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
