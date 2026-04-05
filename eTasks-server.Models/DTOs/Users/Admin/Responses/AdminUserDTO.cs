using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Users.Admin.Responses
{
    public class AdminUserDTO
    {
        public Guid Uid { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuario precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuario nao pode exceder 30 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        public string? PhotoPath { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAccessAt { get; set; }
    }
}
