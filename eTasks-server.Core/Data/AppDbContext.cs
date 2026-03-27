using eTasks_server.Models.Version;
using eTasks_server.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.Data
{
    /// <summary>
    /// Contexto do banco de dados para a aplicação eTasks-server, utilizando Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        #region DbSets
        public DbSet<eTasksVersion> DbVersion { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Configurations
            modelBuilder.Entity<eTasksVersion>().ToTable("version").HasKey(x => x.Id);

            modelBuilder.Entity<User>().ToTable("users").HasKey(x => x.Uid);
            modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens").HasKey(x => x.Id);
            modelBuilder.Entity<PasswordResetCode>().ToTable("password_reset_codes").HasKey(x => x.Id);
            modelBuilder.Entity<LoginLog>().ToTable("login_logs").HasKey(x => x.Id);
            #endregion

            base.OnModelCreating(modelBuilder);
        }
    }
}
