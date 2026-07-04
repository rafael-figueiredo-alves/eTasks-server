using eTasks_server.Models.Enums.Users;

namespace eTasks_server.Models.DTOs.Users.Profile.Requests
{
    /// <summary>
    /// Dados enviados para atualizar as configuracoes do usuario.
    /// </summary>
    public class PatchUserSettingsRequest
    {
        /// <summary>
        /// Atualiza o tema do aplicativo.
        /// </summary>
        public string? Theme { get; set; }

        /// <summary>
        /// Atualiza o idioma do aplicativo.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Atualiza a tela inicial preferida do aplicativo.
        /// </summary>
        public AppStartScreen? InitialScreen { get; set; }

        /// <summary>
        /// Atualiza a ativacao do sistema de bonus.
        /// </summary>
        public bool? EnableBonusSystem { get; set; }
    }
}
