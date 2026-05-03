using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    public class ExternalAuthSession : IEntityModelConfiguration<ExternalAuthSession>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid SessionCode { get; set; } = Guid.CreateVersion7();
        public string Provider { get; set; } = string.Empty;
        public string ClientUserAgent { get; set; } = string.Empty;
        public string ClientInstanceId { get; set; } = string.Empty;
        public string FixedStateCode { get; set; } = string.Empty;
        public string Status { get; set; } = ExternalAuthSessionStatus.Pending;
        public string? ErrorCode { get; set; }
        public string? ErrorDescription { get; set; }
        public string? ProtectedLoginResponseJson { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
        public DateTime? CompletedAt { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExternalAuthSession>()
                .ToTable("external_auth_sessions")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ExternalAuthSession>()
                .HasIndex(x => x.SessionCode)
                .IsUnique();

            modelBuilder.Entity<ExternalAuthSession>()
                .HasIndex(x => new { x.Provider, x.ClientUserAgent, x.ClientInstanceId });

            modelBuilder.Entity<ExternalAuthSession>()
                .HasIndex(x => new { x.Status, x.ExpiresAt });
        }
    }

    public static class ExternalAuthSessionStatus
    {
        public const string Pending = "Pending";
        public const string Success = "Success";
        public const string Failed = "Failed";
        public const string Consumed = "Consumed";
    }
}
