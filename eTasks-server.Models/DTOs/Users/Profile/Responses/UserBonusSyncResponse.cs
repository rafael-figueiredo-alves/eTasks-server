namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserBonusSyncResponse
    {
        public int TotalPoints { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public List<UserBonusPointEntryDTO> PointEntries { get; set; } = [];
        public List<UserAchievementDTO> Achievements { get; set; } = [];
    }
}
