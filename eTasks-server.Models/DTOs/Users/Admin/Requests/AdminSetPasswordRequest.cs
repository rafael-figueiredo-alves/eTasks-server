using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Users.Admin.Requests
{
    public class AdminSetPasswordRequest
    {
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha nao deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
