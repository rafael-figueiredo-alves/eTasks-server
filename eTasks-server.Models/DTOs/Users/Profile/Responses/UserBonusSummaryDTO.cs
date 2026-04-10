namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Resumo dos pontos obtidos pelo usuário e suas conquistas relacionadas a esses pontos.
    /// </summary>
    public class UserBonusSummaryDTO
    {
        /// <summary>
        /// Total de pontos acumulados pelo usuário, que podem ser usados para resgatar recompensas ou alcançar novos níveis de conquistas.
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// Lista de conquistas relacionadas aos pontos acumulados pelo usuário, incluindo detalhes como o nome da conquista, descrição e data de obtenção.
        /// </summary>
        public List<UserAchievementDTO> Achievements { get; set; } = [];
    }
}
