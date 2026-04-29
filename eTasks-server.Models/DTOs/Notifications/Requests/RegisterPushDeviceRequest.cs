using eTasks_server.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Notifications.Requests
{
    public class RegisterPushDeviceRequest
    {
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        public PushDevicePlatform Platform { get; set; } = PushDevicePlatform.Other;
        public string? DisplayName { get; set; }
        public string? PushToken { get; set; }
        public string? PushEndpoint { get; set; }
        public string? P256dh { get; set; }
        public string? Auth { get; set; }
    }
}
