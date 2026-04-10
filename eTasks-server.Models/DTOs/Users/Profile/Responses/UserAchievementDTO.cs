namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Entidade DTO com dados sobre as conquistas do usuário, incluindo código, nome, pontos necessários, tipo de exibição e data de conquista.
    /// </summary>
    public class UserAchievementDTO
    {
        /// <summary>
        /// Código único da conquista, utilizado para identificar a conquista de forma exclusiva.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nome da conquista, utilizado para exibir a conquista de forma legível para o usuário.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Pontos necessários para alcançar a conquista, utilizado para determinar quando o usuário alcançou a conquista com base em seus pontos acumulados.
        /// </summary>
        public int PointsRequired { get; set; }

        /// <summary>
        /// Tipo de imagem a exibir para a conquista, utilizado para determinar qual imagem deve ser exibida para o usuário com base no tipo de exibição da conquista.
        /// </summary>
        public int DisplayType { get; set; }

        /// <summary>
        /// Data e hora em que a conquista foi alcançada, utilizado para exibir a data de conquista para o usuário e para determinar a ordem de exibição das conquistas com base na data de conquista.
        /// </summary>
        public DateTime AchievedAt { get; set; }
    }
}
