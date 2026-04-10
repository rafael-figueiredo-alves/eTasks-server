using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Users.Admin.Requests
{
    /// <summary>
    /// Entidade de solicitação para a operação de definição de senha por parte do administrador, permitindo que o administrador defina uma nova senha para um usuário específico.
    /// </summary>
    public class AdminSetPasswordRequest
    {
        /// <summary>
        /// Nova senha a ser definida para o usuário, fornecida pelo administrador. Esta senha deve atender aos requisitos de segurança, como comprimento mínimo e máximo, para garantir a proteção da conta do usuário.
        /// </summary>
        [Required]
        [PasswordPropertyText]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [MaxLength(30, ErrorMessage = "A senha nao deve exceder 30 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
