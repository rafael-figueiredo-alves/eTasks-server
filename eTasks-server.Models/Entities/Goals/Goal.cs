using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Goals
{
    /// <summary>
    /// Representa uma meta ou objetivo definido pelo usuario.
    /// </summary>
    public class Goal : IEntityModelConfiguration<Goal>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? Description { get; set; }
        public GoalType Type { get; set; } = GoalType.Personal;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int? RewardPoints { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.Active;
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>()
                .ToTable("goals")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Goal>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .Property(x => x.Type)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .Property(x => x.Priority)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .HasIndex(x => new { x.UserUid, x.Status });

            modelBuilder.Entity<Goal>()
                .HasIndex(x => new { x.UserUid, x.IsDeleted });
        }
    }
}
