using eTasks_server.Models.Entities.Gamification;

namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    public class UserBonusPointEntryDTO
    {
        public Guid Id { get; set; }
        public int Points { get; set; }
        public BonusPointSource Source { get; set; }
        public string? Description { get; set; }
        public Guid? SourceReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
