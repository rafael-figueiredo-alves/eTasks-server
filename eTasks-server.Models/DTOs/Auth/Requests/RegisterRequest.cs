using eTasks_server.Models.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTO para registrar um novo usuário. Contém as informações necessárias para criar uma conta, como nome, email, senha, foto (opcional) e user agent. As validações garantem que os dados estejam no formato correto e atendam aos requisitos mínimos de segurança e formato.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nome de exibição do usuário. Deve conter entre 3 e 30 caracteres. Este campo é obrigatório para criar uma nova conta e será usado para identificar o usuário na plataforma.
        /// </summary>
        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuario precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuario nao pode exceder 30 caracteres")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do usuário. Deve ser um endereço de e-mail válido e é obrigatório para criar uma nova conta. O e-mail será usado para autenticação e comunicação com o usuário, além de ser um identificador único na plataforma.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário para autenticação. Deve conter entre 6 e 30 caracteres. Este campo é obrigatório para criar uma nova conta e deve ser protegido adequadamente para garantir a segurança do usuário. A senha será usada para autenticar o usuário durante o login e deve ser armazenada de forma segura no banco de dados.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha nao deve exceder 30 caracteres")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Imagem de perfil do usuário em formato Base64. Este campo é opcional e pode ser usado para fornecer uma foto de perfil para o usuário. A validação personalizada garante que a string fornecida esteja no formato Base64, que é comumente usado para representar imagens em texto. Se fornecida, a imagem será associada à conta do usuário e poderá ser exibida em seu perfil.
        /// </summary>
        [Base64String(ErrorMessage = "Formato de imagem aceito é apenas Base64.")]
        public string? PhotoBase64 { get; set; }

        /// <summary>
        /// Identificador do user agent do cliente, que deve ser uma string não vazia. Este campo é obrigatório e deve passar pela validação personalizada de user agent. O user agent é usado para identificar o tipo de dispositivo ou navegador que o usuário está usando, o que pode ser útil para fins de segurança e análise de uso.
        /// </summary>
        [Required]
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
    }
}
