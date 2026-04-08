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
            eTasksVersion.Configure(modelBuilder);
            User.Configure(modelBuilder);
            RefreshToken.Configure(modelBuilder);
            PasswordResetCode.Configure(modelBuilder);
            LoginLog.Configure(modelBuilder);
            global::eTasks_server.Models.Entities.Users.UserSettings.Configure(modelBuilder);
            UserBonusPoint.Configure(modelBuilder);
            BonusAchievement.Configure(modelBuilder);
            UserAchievement.Configure(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}
