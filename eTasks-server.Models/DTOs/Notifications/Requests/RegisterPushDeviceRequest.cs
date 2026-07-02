using eTasks_server.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Notifications.Requests
{
    /// <summary>
    /// Requesição para registrar um dispositivo de push para notificações.
    /// </summary>
    public class RegisterPushDeviceRequest
    {
        /// <summary>
        /// Identificador único do dispositivo.
        /// </summary>
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Plataforma do dispositivo (Android, iOS, Web, etc.).
        /// </summary>
        public PushDevicePlatform Platform { get; set; } = PushDevicePlatform.Other;

        /// <summary>
        /// Nome de exibição do dispositivo (opcional).
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Token de push do dispositivo (opcional).
        /// </summary>
        public string? PushToken { get; set; }

        /// <summary>
        /// Endpoint de push do dispositivo (opcional).
        /// </summary>
        public string? PushEndpoint { get; set; }

        /// <summary>
        /// Chave pública P256dh do dispositivo para notificações push (opcional).
        /// </summary>
        public string? P256dh { get; set; }

        /// <summary>
        /// Chave de autenticação do dispositivo para notificações push (opcional).
        /// </summary>
        public string? Auth { get; set; }
    }
}
