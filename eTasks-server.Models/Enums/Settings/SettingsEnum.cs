using eTasks_server.Models.Enums.Users;

namespace eTasks_server.Models.Enums.Settings
{
    /// <summary>
    /// Enumerado de configurações
    /// </summary>
    public static class SettingsEnum
    {
        /// <summary>
        /// Temas permitidos
        /// </summary>
        public static readonly HashSet<string> AllowedThemes = new(StringComparer.OrdinalIgnoreCase) { "light", "dark" };

        /// <summary>
        /// Idiomas permitidos
        /// </summary>
        public static readonly HashSet<string> AllowedLanguages = new(StringComparer.OrdinalIgnoreCase) { "pt-BR", "en-US" };

        /// <summary>
        /// Tema padrão
        /// </summary>
        public static readonly string DefaultTheme = "light";

        /// <summary>
        /// Idioma padrão
        /// </summary>
        public static readonly string DefaultLanguage = "pt-BR";

        /// <summary>
        /// Valor padão para tela inicial
        /// </summary>
        public static readonly AppStartScreen DefaultStartScreen = AppStartScreen.Home;

        /// <summary>
        /// Valor padrão do status de habilitação da bonificação do app
        /// </summary>
        public static readonly bool DefaultEnableBonus = false;

        /// <summary>
        /// URL base padrão do sistema
        /// </summary>
        public static readonly string DefaultBaseURL = "http://localhost:5033";
    }
}
