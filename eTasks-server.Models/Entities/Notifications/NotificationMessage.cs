using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Notifications;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notifications
{
    public class NotificationMessage : IEntityModelConfiguration<NotificationMessage>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid? CreatedByUserUid { get; set; }
        public NotificationTargetType TargetType { get; set; } = NotificationTargetType.All;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? DataJson { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public User? CreatedByUser { get; set; }
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationMessage>()
                .ToTable("notification_messages")
                .HasKey(x => x.Id);

            modelBuilder.Entity<NotificationMessage>()
                .Property(x => x.TargetType)
                .HasConversion<int>();

            modelBuilder.Entity<NotificationMessage>()
                .Property(x => x.Title)
                .HasMaxLength(120);

            modelBuilder.Entity<NotificationMessage>()
                .Property(x => x.Body)
                .HasMaxLength(500);

            modelBuilder.Entity<NotificationMessage>()
                .HasMany(x => x.Recipients)
                .WithOne(x => x.Message)
                .HasForeignKey(x => x.NotificationMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
