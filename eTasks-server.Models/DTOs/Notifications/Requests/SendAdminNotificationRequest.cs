using eTasks_server.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Notifications.Requests
{
    /// <summary>
    /// Requisição para enviar uma notificação administrativa para os usuários.
    /// </summary>
    public class SendAdminNotificationRequest
    {
        /// <summary>
        /// Tipo de destino da notificação (Todos os usuários, Usuários específicos, etc.).
        /// </summary>
        public NotificationTargetType TargetType { get; set; } = NotificationTargetType.All;

        /// <summary>
        /// Lista de UIDs dos usuários que receberão a notificação. Deve ser preenchida apenas se o TargetType for UserUids.
        /// </summary>
        public List<Guid> UserUids { get; set; } = [];

        /// <summary>
        /// Título da notificação.
        /// </summary>
        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Corpo da notificação.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Body { get; set; } = string.Empty;
        
        /// <summary>
        /// URL da ação da notificação (opcional).
        /// </summary>
        public string? ActionUrl { get; set; }
        
        /// <summary>
        /// Dados JSON para a notificação (opcional).
        /// </summary>
        public string? DataJson { get; set; }
    }
}
