using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notifications
{
    /// <summary>
    /// Destinatário da notificação
    /// </summary>
    public class NotificationRecipient : IEntityModelConfiguration<NotificationRecipient>
    {
        /// <summary>
        /// Identificação
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificação da mensagem de notificação
        /// </summary>
        public Guid NotificationMessageId { get; set; }

        /// <summary>
        /// Identificador de usuário
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Data/Hora de criação
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data/Hora quando foi lida
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Mensagem
        /// </summary>
        public NotificationMessage? Message { get; set; }

        /// <summary>
        /// Usuário
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configurações
        /// </summary>
        /// <param name="modelBuilder"></param>
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
