using eTasks_server.Models.Entities;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    public class UserAchievement : IEntityModelConfiguration<UserAchievement>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public Guid BonusAchievementId { get; set; }
        public int PointsAtAchievement { get; set; }
        public DateTime AchievedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }
        public BonusAchievement? BonusAchievement { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAchievement>().ToTable("user_achievements").HasKey(x => x.Id);

            modelBuilder.Entity<UserAchievement>()
                .HasOne(x => x.BonusAchievement)
                .WithMany(x => x.UserAchievements)
                .HasForeignKey(x => x.BonusAchievementId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAchievement>()
                .HasIndex(x => new { x.UserUid, x.BonusAchievementId })
                .IsUnique();
        }
    }
}
