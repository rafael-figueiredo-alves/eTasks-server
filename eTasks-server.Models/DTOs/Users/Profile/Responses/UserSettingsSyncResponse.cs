using eTasks_server.Models.Enums.Users;

namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// Retorno de sincronização de configurações de usuário
    /// </summary>
    public class UserSettingsSyncResponse
    {
        /// <summary>
        /// Identificação
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do usuário
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Tema visual
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Idioma
        /// </summary>
        public string Language { get; set; } = "pt-BR";

        /// <summary>
        /// Tela inicial
        /// </summary>
        public AppStartScreen InitialScreen { get; set; } = AppStartScreen.Home;

        /// <summary>
        /// Habilita sistema de bonificação
        /// </summary>
        public bool EnableBonusSystem { get; set; }

        /// <summary>
        /// Data/Hora de inserção
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data/Hora de atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
