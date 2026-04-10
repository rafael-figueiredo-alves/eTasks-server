namespace eTasks_server.Models.DTOs.Users.Profile.Requests
{
    /// <summary>
    /// Entidade enviada para atualizar as configurações do usuário. Todos os campos são opcionais, permitindo que o usuário atualize apenas as configurações desejadas.
    /// </summary>
    public class PatchUserSettingsRequest
    {
        /// <summary>
        /// Atualiza o tema do aplicativo. O valor pode ser "light", "dark" ou "system", permitindo que o usuário escolha entre um tema claro, escuro ou seguir as configurações do sistema operacional.
        /// </summary>
        public string? Theme { get; set; }

        /// <summary>
        /// Atualiza o idioma do aplicativo. O valor deve ser um código de idioma válido, como "en" para inglês, "pt" para português, etc., permitindo que o usuário escolha o idioma de sua preferência.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Atualiza se o aplicativo deve usar a câmera para funcionalidades como leitura de QR codes ou captura de fotos. O valor é um booleano, onde "true" habilita o uso da câmera e "false" desabilita, permitindo que o usuário controle o acesso à câmera conforme suas preferências de privacidade.
        /// </summary>
        public bool? UseCamera { get; set; }

        /// <summary>
        /// Indicador se a aplicação deve habilitar o sistema de pontos e recompensas para o usuário. O valor é um booleano, onde "true" ativa o sistema de bônus, permitindo que o usuário acumule pontos e receba recompensas com base em suas atividades dentro do aplicativo, enquanto "false" desativa essa funcionalidade.
        /// </summary>
        public bool? EnableBonusSystem { get; set; }
    }
}
