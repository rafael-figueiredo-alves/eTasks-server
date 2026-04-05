using eTasks_server.Models.Utils;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Users
{
    public class User
    {
        public Guid Uid { get; set; } = Guid.CreateVersion7();

        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuario precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuario nao pode exceder 30 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime? LastAccessAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = new List<PasswordResetCode>();
        public UserSettings? Settings { get; set; }
        public ICollection<UserBonusPoint> BonusPoints { get; set; } = new List<UserBonusPoint>();
        public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    }
}
