namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserBonusSummaryDTO
    {
        public int TotalPoints { get; set; }
        public List<UserAchievementDTO> Achievements { get; set; } = [];
    }
}
