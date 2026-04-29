using eTasks_server.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Notifications.Requests
{
    public class SendAdminNotificationRequest
    {
        public NotificationTargetType TargetType { get; set; } = NotificationTargetType.All;
        public List<Guid> UserUids { get; set; } = [];

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Body { get; set; } = string.Empty;

        public string? ActionUrl { get; set; }
        public string? DataJson { get; set; }
    }
}
