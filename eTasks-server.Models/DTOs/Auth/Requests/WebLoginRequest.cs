using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTRO para a solicitação de login via web, contendo os campos necessários para autenticação do usuário, como email, senha e opção de "lembrar-me".
    /// </summary>
    public class WebLoginRequest
    {
        /// <summary>
        /// Email do usuário, que deve ser um endereço de e-mail válido. Este campo é obrigatório para a autenticação do usuário.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário, que deve ter entre 6 e 30 caracteres. Este campo é obrigatório para a autenticação do usuário.
        /// </summary>
        [Required]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha nao deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o usuário deseja que a sessão seja mantida ativa mesmo após fechar o navegador. Se verdadeiro, a sessão permanecerá ativa por um período prolongado, permitindo que o usuário permaneça logado sem precisar inserir suas credenciais novamente.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
