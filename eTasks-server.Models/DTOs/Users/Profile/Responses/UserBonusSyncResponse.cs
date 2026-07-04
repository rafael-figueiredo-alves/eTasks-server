namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Retorno da sincronização dos Bonus do usuário
    /// </summary>
    public class UserBonusSyncResponse
    {
        /// <summary>
        /// Pontos totais
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// Última atualização
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Entradas de pontos
        /// </summary>
        public List<UserBonusPointEntryDTO> PointEntries { get; set; } = [];

        /// <summary>
        /// Lista de conquistas
        /// </summary>
        public List<UserAchievementDTO> Achievements { get; set; } = [];
    }
}
