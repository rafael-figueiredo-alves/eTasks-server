namespace eTasks_server.Models.DTOs.Notifications.Responses
{
    /// <summary>
    /// Resposta do envio de notificação para administradores.
    /// </summary>
    public class SendAdminNotificationResponse
    {
        /// <summary>
        /// Identificador único da notificação enviada.
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// Número de destinatários que receberam a notificação.
        /// </summary>
        public int RecipientCount { get; set; }

        /// <summary>
        /// Número de dispositivos registrados que receberam a notificação.
        /// </summary>
        public int RegisteredDeviceCount { get; set; }

        /// <summary>
        /// Data e hora em que a notificação foi criada.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
