using eTasks_server.Models.Utils;

namespace eTasks_server.Models.Entities.Users
{
    public class UserAchievement
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public Guid BonusAchievementId { get; set; }
        public int PointsAtAchievement { get; set; }
        public DateTime AchievedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }
        public BonusAchievement? BonusAchievement { get; set; }
    }
}
