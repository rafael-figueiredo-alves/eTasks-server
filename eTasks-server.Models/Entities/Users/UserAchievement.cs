using eTasks_server.Models.Entities;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Conquista do usuário
    /// </summary>
    public class UserAchievement : IEntityModelConfiguration<UserAchievement>
    {
        /// <summary>
        /// Identificador único
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// identificador do usuário
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Identificador da conquista bônus
        /// </summary>
        public Guid BonusAchievementId { get; set; }

        /// <summary>
        /// Pontos obtidos ao alcançar a conquista
        /// </summary>
        public int PointsAtAchievement { get; set; }

        /// <summary>
        /// Quando a conquista foi alcançada
        /// </summary>
        public DateTime AchievedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Informações do usuário atrelado a conquista
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Conquista bônus atrelada a conquista do usuário
        /// </summary>
        public BonusAchievement? BonusAchievement { get; set; }

        /// <summary>
        /// Configurar a entidade UserAchievement
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAchievement>()
                                    .ToTable("user_achievements")
                                    .HasKey(x => x.Id);

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
