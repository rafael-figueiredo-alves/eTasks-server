using eTasks_server.Models.Utils;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Users
{
    public class PasswordResetCode
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid UserUid { get; set; }

        [Length(6, 6, ErrorMessage = "O codigo de verificacao deve ter exatamente 6 digitos")]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        public User? User { get; set; }
    }
}
