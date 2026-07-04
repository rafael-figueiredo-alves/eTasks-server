namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Sincronização dos dados do usuário
    /// </summary>
    public class UserDataSyncResponse
    {
        /// <summary>
        /// Horário do servidor
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Configurações
        /// </summary>
        public UserSettingsSyncResponse? Settings { get; set; }

        /// <summary>
        /// Bonus
        /// </summary>
        public UserBonusSyncResponse? Bonus { get; set; }
    }
}
