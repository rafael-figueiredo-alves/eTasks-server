using eTasks_server.Models.Entities.Users;

namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// DTO com as configuracoes do usuario.
    /// </summary>
    public class UserSettingsDTO
    {
        /// <summary>
        /// Tema visual preferido.
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Idioma preferido.
        /// </summary>
        public string Language { get; set; } = "pt-BR";

        /// <summary>
        /// Tela inicial preferida.
        /// </summary>
        public AppStartScreen InitialScreen { get; set; } = AppStartScreen.Home;

        /// <summary>
        /// Indica se o sistema de bonus esta habilitado.
        /// </summary>
        public bool EnableBonusSystem { get; set; }
    }
}
