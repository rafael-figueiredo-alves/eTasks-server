using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTO para solicitar a redefinição de senha. Contém o endereço de e-mail do usuário que deseja redefinir a senha.
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// E-mail do usuário que deseja redefinir a senha. Deve ser um endereço de e-mail válido e é obrigatório.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Só é aceito endereço de e-mail válido")]
        public string Email { get; set; } = string.Empty;
    }
}
