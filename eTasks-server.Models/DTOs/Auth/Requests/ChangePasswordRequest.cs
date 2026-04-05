using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha atual deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha atual nao deve exceder 30 caracteres")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A nova senha nao deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
