namespace eTasks_server.Models.Enums.Notifications
{
    /// <summary>
    /// Plataformas de dispositivos para os quais as notificações push podem ser enviadas. Cada valor representa um tipo específico de dispositivo ou plataforma, permitindo que o sistema de notificações direcione as mensagens de forma adequada com base no dispositivo do usuário.
    /// </summary>
    public enum PushDevicePlatform
    {
        /// <summary>
        /// Aplicativo Web Progressivo (PWA) - Uma plataforma que permite que aplicativos web sejam instalados e usados como aplicativos nativos em dispositivos móveis e desktops, oferecendo uma experiência de usuário semelhante a um aplicativo tradicional.
        /// </summary>
        Pwa = 1,

        /// <summary>
        /// Delphi para Windows - Refere-se a aplicativos desenvolvidos usando a linguagem de programação Delphi, especificamente para a plataforma Windows. Esses aplicativos podem ser nativos e aproveitar os recursos do sistema operacional Windows.
        /// </summary>
        DelphiWindows = 2,

        /// <summary>
        /// Delphi para Android - Refere-se a aplicativos desenvolvidos usando a linguagem de programação Delphi, especificamente para a plataforma Android. Esses aplicativos podem ser nativos e aproveitar os recursos do sistema operacional Android.
        /// </summary>
        DelphiAndroid = 3,
        
        /// <summary>
        /// Dispositivos Android
        /// </summary>
        Android = 4,

        /// <summary>
        /// Dispositivos Windows - Refere-se a qualquer dispositivo que execute o sistema operacional Windows, incluindo desktops, laptops e tablets. As notificações push para esta plataforma podem ser direcionadas a aplicativos nativos ou a aplicativos universais do Windows (UWP).
        /// </summary>
        Windows = 5,

        /// <summary>
        /// Outras plataformas - Este valor é usado para representar qualquer plataforma de dispositivo que não se encaixe nas categorias específicas listadas acima. Pode incluir plataformas emergentes ou menos comuns para as quais o sistema de notificações ainda oferece suporte, garantindo flexibilidade para futuras expansões ou integrações com novos tipos de dispositivos.
        /// </summary>
        Other = 99
    }
}
