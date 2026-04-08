using eTasks_server.Models.Entities;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    public class UserBonusPoint : IEntityModelConfiguration<UserBonusPoint>
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public int Points { get; set; }
        public string? Source { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }

        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserBonusPoint>().ToTable("user_bonus_points").HasKey(x => x.Id);
        }
    }
}
