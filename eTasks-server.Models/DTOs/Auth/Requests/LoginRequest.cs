using eTasks_server.Models.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha nao deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
    }
}
