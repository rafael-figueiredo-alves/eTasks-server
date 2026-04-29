using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notifications
{
    public class NotificationRecipient : IEntityModelConfiguration<NotificationRecipient>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid NotificationMessageId { get; set; }
        public Guid UserUid { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime? ReadAt { get; set; }
        public NotificationMessage? Message { get; set; }
        public User? User { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationRecipient>()
                .ToTable("notification_recipients")
                .HasKey(x => x.Id);

            modelBuilder.Entity<NotificationRecipient>()
                .HasIndex(x => new { x.UserUid, x.ReadAt });

            modelBuilder.Entity<NotificationRecipient>()
                .HasIndex(x => new { x.NotificationMessageId, x.UserUid })
                .IsUnique();
        }
    }
}
