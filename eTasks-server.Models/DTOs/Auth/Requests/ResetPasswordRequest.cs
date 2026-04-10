using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTO para solicitar a redefinição de senha. Contém o e-mail do usuário, o código de verificação e a nova senha.
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Email do usuário para o qual a senha será redefinida. Deve ser um endereço de e-mail válido.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Código de verificação enviado para o e-mail do usuário. Deve conter exatamente 6 dígitos.
        /// </summary>
        [Required]
        [Length(6, 6, ErrorMessage = "O codigo de verificacao deve ter exatamente 6 digitos")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha que o usuário deseja definir. Deve ter entre 6 e 30 caracteres.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A nova senha nao deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
