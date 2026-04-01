using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Auth
{
    public class WebLoginRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Só é aceito endereço de e-mail válido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha não deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
