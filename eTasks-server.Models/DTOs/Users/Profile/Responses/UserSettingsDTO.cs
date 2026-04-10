namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Entidade de transferência de dados (DTO) para as configurações do usuário, incluindo preferências de tema, idioma, uso da câmera e sistema de bônus.
    /// </summary>
    public class UserSettingsDTO
    {
        /// <summary>
        /// Tema do usuário, com valor padrão "light". Pode ser "light" ou "dark".
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Idioma preferido do usuário, com valor padrão "pt" (Português). Pode ser "pt", "en", "es", etc.
        /// </summary>
        public string Language { get; set; } = "pt";

        /// <summary>
        /// Se usa câmera do aplicativo ou nativa do ambiente
        /// </summary>
        public bool UseCamera { get; set; }

        /// <summary>
        /// Se ativa ou não o sistema de bônus, que pode incluir recompensas, pontos ou outras formas de incentivo para os usuários.
        /// </summary>
        public bool EnableBonusSystem { get; set; }
    }
}
