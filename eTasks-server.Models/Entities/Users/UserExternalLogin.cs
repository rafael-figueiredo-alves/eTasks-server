using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    public class UserExternalLogin : IEntityModelConfiguration<UserExternalLogin>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserExternalLogin>()
                .ToTable("user_external_logins")
                .HasKey(x => x.Id);

            modelBuilder.Entity<UserExternalLogin>()
                .HasIndex(x => new { x.Provider, x.ProviderUserId })
                .IsUnique();

            modelBuilder.Entity<UserExternalLogin>()
                .HasIndex(x => x.UserUid);
        }
    }
}
