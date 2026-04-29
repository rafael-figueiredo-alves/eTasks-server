using eTasks_server.Models.Enums.Notifications;

namespace eTasks_server.Models.DTOs.Notifications.Responses
{
    public class PushDeviceRegistrationResponse
    {
        public Guid Id { get; set; }
        public PushDevicePlatform Platform { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastSeenAt { get; set; }
    }
}
