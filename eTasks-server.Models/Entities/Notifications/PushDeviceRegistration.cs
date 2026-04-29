using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Notifications;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notifications
{
    public class PushDeviceRegistration : IEntityModelConfiguration<PushDeviceRegistration>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public PushDevicePlatform Platform { get; set; } = PushDevicePlatform.Other;
        public string DeviceId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? PushToken { get; set; }
        public string? PushEndpoint { get; set; }
        public string? P256dh { get; set; }
        public string? Auth { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime LastSeenAt { get; set; } = SaoPauloDateTime.Now();
        public User? User { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PushDeviceRegistration>()
                .ToTable("push_device_registrations")
                .HasKey(x => x.Id);

            modelBuilder.Entity<PushDeviceRegistration>()
                .HasIndex(x => new { x.UserUid, x.Platform, x.DeviceId })
                .IsUnique();

            modelBuilder.Entity<PushDeviceRegistration>()
                .Property(x => x.Platform)
                .HasConversion<int>();

            modelBuilder.Entity<PushDeviceRegistration>()
                .Property(x => x.DeviceId)
                .HasMaxLength(150);

            modelBuilder.Entity<PushDeviceRegistration>()
                .Property(x => x.DisplayName)
                .HasMaxLength(120);
        }
    }
}
