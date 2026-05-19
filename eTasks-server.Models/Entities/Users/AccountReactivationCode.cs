using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Users
{
    public class AccountReactivationCode : IEntityModelConfiguration<AccountReactivationCode>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();

        public Guid UserUid { get; set; }

        [MaxLength(128)]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? UsedAt { get; set; }

        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountReactivationCode>()
                .ToTable("account_reactivation_codes")
                .HasKey(x => x.Id);

            modelBuilder.Entity<AccountReactivationCode>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<AccountReactivationCode>()
                .HasIndex(x => new { x.UserUid, x.IsUsed, x.ExpiresAt });
        }
    }
}
