namespace eTasks_server.Models.DTOs.Notifications.Responses
{
    public class SendAdminNotificationResponse
    {
        public Guid NotificationId { get; set; }
        public int RecipientCount { get; set; }
        public int RegisteredDeviceCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
