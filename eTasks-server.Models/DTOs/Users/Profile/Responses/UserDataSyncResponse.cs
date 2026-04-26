namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserDataSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public UserSettingsSyncResponse? Settings { get; set; }
        public UserBonusSyncResponse? Bonus { get; set; }
    }
}
