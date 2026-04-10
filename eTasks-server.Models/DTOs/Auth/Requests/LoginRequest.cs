using eTasks_server.Models.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTO para representar os dados de login do usuário, incluindo email, senha e user agent.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// E-mail do usuário, que deve ser um endereço de e-mail válido. Este campo é obrigatório.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário, que deve ter entre 6 e 30 caracteres. Este campo é obrigatório.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha nao deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do user agent do cliente, que deve ser uma string não vazia. Este campo é obrigatório e deve passar pela validação personalizada de user agent.
        /// </summary>
        [Required]
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
    }
}
