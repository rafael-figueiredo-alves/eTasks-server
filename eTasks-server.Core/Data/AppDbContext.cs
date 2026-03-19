using eTasks_server.Models.Version;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // Define DbSet properties for your entities here
        // Example:
        // public DbSet<User> Users { get; set; }

        public DbSet<eTasksVersion> DbVersion { get; set; }
        public DbSet<eTasks_server.Models.Users.User> Users { get; set; }
        public DbSet<eTasks_server.Models.Users.RefreshToken> RefreshTokens { get; set; }
        public DbSet<eTasks_server.Models.Users.PasswordResetCode> PasswordResetCodes { get; set; }
        public DbSet<eTasks_server.Models.Users.LoginLog> LoginLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<eTasksVersion>().ToTable("version").HasKey(x => x.Id);

            modelBuilder.Entity<eTasks_server.Models.Users.User>().ToTable("users").HasKey(x => x.Uid);
            modelBuilder.Entity<eTasks_server.Models.Users.RefreshToken>().ToTable("refresh_tokens").HasKey(x => x.Id);
            modelBuilder.Entity<eTasks_server.Models.Users.PasswordResetCode>().ToTable("password_reset_codes").HasKey(x => x.Id);
            modelBuilder.Entity<eTasks_server.Models.Users.LoginLog>().ToTable("login_logs").HasKey(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
