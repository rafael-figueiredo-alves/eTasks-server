namespace eTasks_server.Models.DTOs.Notifications.Responses
{
    public class NotificationInboxItemResponse
    {
        public Guid RecipientId { get; set; }
        public Guid NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? DataJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead => ReadAt.HasValue;
    }
}
