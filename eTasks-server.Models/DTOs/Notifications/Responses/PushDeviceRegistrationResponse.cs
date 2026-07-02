using eTasks_server.Models.Enums.Notifications;

namespace eTasks_server.Models.DTOs.Notifications.Responses
{
    /// <summary>
    /// Resposta ao registro de um dispositivo para notificações push.
    /// </summary>
    public class PushDeviceRegistrationResponse
    {
        /// <summary>
        /// Identificador único do dispositivo registrado.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Plataforma do dispositivo registrado (ex: iOS, Android, Web).
        /// </summary>
        public PushDevicePlatform Platform { get; set; }

        /// <summary>
        /// Identificador do dispositivo fornecido pelo serviço de notificações push (ex: token do Firebase, APNs).
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Nome de exibição do dispositivo registrado, usado para identificar o dispositivo de forma amigável.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o dispositivo está ativo e apto a receber notificações push.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Data e hora da última vez que o dispositivo foi visto ou utilizado para receber notificações push.
        /// </summary>
        public DateTime LastSeenAt { get; set; }
    }
}
