using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Entities.Version;
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
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<UserBonusPoint> UserBonusPoints { get; set; }
        public DbSet<BonusAchievement> BonusAchievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Configurations
            modelBuilder.Entity<eTasksVersion>().ToTable("version").HasKey(x => x.Id);

            modelBuilder.Entity<User>().ToTable("users").HasKey(x => x.Uid);
            modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens").HasKey(x => x.Id);
            modelBuilder.Entity<PasswordResetCode>().ToTable("password_reset_codes").HasKey(x => x.Id);
            modelBuilder.Entity<LoginLog>().ToTable("login_logs").HasKey(x => x.Id);
            modelBuilder.Entity<UserSettings>().ToTable("user_settings").HasKey(x => x.Id);
            modelBuilder.Entity<UserBonusPoint>().ToTable("user_bonus_points").HasKey(x => x.Id);
            modelBuilder.Entity<BonusAchievement>().ToTable("bonus_achievements").HasKey(x => x.Id);
            modelBuilder.Entity<UserAchievement>().ToTable("user_achievements").HasKey(x => x.Id);

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(x => x.Settings)
                .WithOne(x => x.User)
                .HasForeignKey<UserSettings>(x => x.UserUid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(x => x.BonusPoints)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserUid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(x => x.Achievements)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserUid)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSettings>()
                .HasIndex(x => x.UserUid)
                .IsUnique();

            modelBuilder.Entity<BonusAchievement>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<UserAchievement>()
                .HasOne(x => x.BonusAchievement)
                .WithMany(x => x.UserAchievements)
                .HasForeignKey(x => x.BonusAchievementId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAchievement>()
                .HasIndex(x => new { x.UserUid, x.BonusAchievementId })
                .IsUnique();
            #endregion

            base.OnModelCreating(modelBuilder);
        }
    }
}
