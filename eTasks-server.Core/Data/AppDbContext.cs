using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Goals;
using eTasks_server.Models.Entities.Notes;
using eTasks_server.Models.Entities.Notifications;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Readings;
using eTasks_server.Models.Entities.Settings;
using eTasks_server.Models.Entities.Shopping;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Entities.Version;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
        public DbSet<UserExternalLogin> UserExternalLogins { get; set; }
        public DbSet<ExternalAuthSession> ExternalAuthSessions { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<UserBonusPoint> UserBonusPoints { get; set; }
        public DbSet<BonusAchievement> BonusAchievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<BonusPointRule> BonusPointRules { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskRecurrence> TaskRecurrences { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
        public DbSet<NoteItem> Notes { get; set; }
        public DbSet<ReadingItem> ReadingItems { get; set; }
        public DbSet<FinanceEntry> FinanceEntries { get; set; }
        public DbSet<FinanceRecurrence> FinanceRecurrences { get; set; }
        public DbSet<ServerSettings> ServerSettings { get; set; }
        public DbSet<PushDeviceRegistration> PushDeviceRegistrations { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            eTasksVersion.Configure(modelBuilder);
            User.Configure(modelBuilder);
            RefreshToken.Configure(modelBuilder);
            UserExternalLogin.Configure(modelBuilder);
            ExternalAuthSession.Configure(modelBuilder);
            PasswordResetCode.Configure(modelBuilder);
            LoginLog.Configure(modelBuilder);
            global::eTasks_server.Models.Entities.Users.UserSettings.Configure(modelBuilder);
            UserBonusPoint.Configure(modelBuilder);
            BonusAchievement.Configure(modelBuilder);
            UserAchievement.Configure(modelBuilder);
            BonusPointRule.Configure(modelBuilder);
            TaskItem.Configure(modelBuilder);
            TaskRecurrence.Configure(modelBuilder);
            Goal.Configure(modelBuilder);
            ShoppingList.Configure(modelBuilder);
            ShoppingListItem.Configure(modelBuilder);
            NoteItem.Configure(modelBuilder);
            ReadingItem.Configure(modelBuilder);
            FinanceEntry.Configure(modelBuilder);
            FinanceRecurrence.Configure(modelBuilder);
            global::eTasks_server.Models.Entities.Settings.ServerSettings.Configure(modelBuilder);
            PushDeviceRegistration.Configure(modelBuilder);
            NotificationMessage.Configure(modelBuilder);
            NotificationRecipient.Configure(modelBuilder);
            ConfigureGuidColumns(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigureGuidColumns(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(Guid) || property.ClrType == typeof(Guid?))
                    {
                        property.SetColumnType("binary(16)");
                    }
                }

                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey?.Properties.Count != 1)
                {
                    continue;
                }

                var keyProperty = primaryKey.Properties[0];
                if (keyProperty.ClrType != typeof(Guid))
                {
                    continue;
                }

                if (keyProperty.Name is "Id" or "Uid")
                {
                    keyProperty.ValueGenerated = ValueGenerated.OnAdd;
                    keyProperty.SetDefaultValueSql("UUID_TO_BIN(UUID(), 1)");
                }
            }
        }
    }
}
