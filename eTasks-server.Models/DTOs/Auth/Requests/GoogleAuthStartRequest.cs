using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// Classse que representa uma solicitação de início de autenticação do Google.
    /// </summary>
    public class GoogleAuthStartRequest
    {
        /// <summary>
        /// Agente do usuário que está fazendo a solicitação de autenticação.
        /// </summary>
        [Required]
        [AllowedUserAgent]
        public string UserAgent { get; set; } = Constants.WebUserAgent;

        /// <summary>
        /// Identificador da instância do cliente que está fazendo a solicitação de autenticação.
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(120)]
        public string ClientInstanceId { get; set; } = string.Empty;


        /// <summary>
        /// URL de retorno para onde o usuário deve ser redirecionado após a autenticação.
        /// </summary>
        [MaxLength(500)]
        public string? ReturnUrl { get; set; }
    }
}
