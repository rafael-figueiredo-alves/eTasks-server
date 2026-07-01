using eTasks_server.Models.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// Classe que representa a solicitação de consumo de autenticação do Google.
    /// </summary>
    public class GoogleAuthConsumeRequest
    {
        /// <summary>
        /// Código da sessão de autenticação do Google.
        /// </summary>
        [Required]
        public Guid SessionCode { get; set; }

        /// <summary>
        /// Agente do usuário que está fazendo a solicitação de autenticação.
        /// </summary>
        [Required]
        [AllowedUserAgent]
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Identificador da instância do cliente que está fazendo a solicitação de autenticação.
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(120)]
        public string ClientInstanceId { get; set; } = string.Empty;
    }
}
