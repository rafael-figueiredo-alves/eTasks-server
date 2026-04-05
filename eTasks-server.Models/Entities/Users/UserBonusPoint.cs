using eTasks_server.Models.Utils;

namespace eTasks_server.Models.Entities.Users
{
    public class UserBonusPoint
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }
        public int Points { get; set; }
        public string? Source { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }
    }
}
