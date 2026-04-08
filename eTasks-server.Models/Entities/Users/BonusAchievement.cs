using eTasks_server.Models.Entities;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    public class BonusAchievement : IEntityModelConfiguration<BonusAchievement>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsRequired { get; set; }
        public AchievementDisplayType DisplayType { get; set; } = AchievementDisplayType.Trophy;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BonusAchievement>().ToTable("bonus_achievements").HasKey(x => x.Id);

            modelBuilder.Entity<BonusAchievement>()
                .Property(x => x.DisplayType)
                .HasConversion<int>();

            modelBuilder.Entity<BonusAchievement>()
                .HasIndex(x => x.Code)
                .IsUnique();
        }
    }
}
