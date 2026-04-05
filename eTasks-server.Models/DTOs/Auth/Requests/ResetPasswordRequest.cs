using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Length(6, 6, ErrorMessage = "O codigo de verificacao deve ter exatamente 6 digitos")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A nova senha nao deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
