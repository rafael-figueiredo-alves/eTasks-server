using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTO para solicitação de mudança de senha. Contém a senha atual e a nova senha, ambas com validação de comprimento e formato.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Senha atual do usuário. Deve ser fornecida para autenticar a solicitação de mudança de senha. A senha deve ter entre 6 e 30 caracteres.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha atual deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha atual nao deve exceder 30 caracteres")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha que o usuário deseja definir. Deve ser diferente da senha atual e atender aos requisitos de comprimento (entre 6 e 30 caracteres). Esta senha será usada para futuras autenticações.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A nova senha nao deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
