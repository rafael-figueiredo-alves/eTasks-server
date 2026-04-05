namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserAchievementDTO
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int PointsRequired { get; set; }
        public DateTime AchievedAt { get; set; }
    }
}
