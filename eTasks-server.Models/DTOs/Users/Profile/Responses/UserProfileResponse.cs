namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Entidade de resposta para o perfil do usuário, contendo informações básicas, configurações e resumo de bônus.
    /// </summary>
    public class UserProfileResponse
    {
        /// <summary>
        /// Identificador único do usuário, utilizado para referência em operações relacionadas ao perfil.
        /// </summary>
        public Guid Uid { get; set; }

        /// <summary>
        /// Nome de exibição do usuário, que pode ser utilizado para personalizar a experiência do usuário e exibir em interfaces de usuário.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// E-mail do usuário, utilizado para comunicação e identificação do usuário dentro do sistema.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação do perfil do usuário, que pode ser utilizada para fins de auditoria e para exibir informações sobre a antiguidade do usuário no sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data e hora do último acesso do usuário ao sistema, que pode ser utilizada para monitorar a atividade do usuário e para fins de segurança.
        /// </summary>
        public DateTime? LastAccessAt { get; set; }

        /// <summary>
        /// Base 64 da foto do perfil do usuário, que pode ser utilizada para exibir a imagem do usuário em interfaces de usuário. O valor pode ser nulo caso o usuário não tenha uma foto de perfil definida.
        /// </summary>
        public string? PhotoBase64 { get; set; }

        /// <summary>
        /// Configurações do usuário, que podem incluir preferências de notificação, temas de interface, e outras opções personalizáveis que afetam a experiência do usuário dentro do sistema.
        /// </summary>
        public UserSettingsDTO Settings { get; set; } = new();

        /// <summary>
        /// Resumo de bônus do usuário, que pode incluir informações sobre pontos acumulados, recompensas disponíveis, e outras métricas relacionadas ao sistema de bônus do aplicativo. Essas informações podem ser utilizadas para incentivar a participação do usuário e para fornecer feedback sobre seu engajamento com o sistema.
        /// </summary>
        public UserBonusSummaryDTO Bonus { get; set; } = new();
    }
}
