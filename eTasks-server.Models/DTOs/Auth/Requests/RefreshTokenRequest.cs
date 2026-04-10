using eTasks_server.Models.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// DTO para solicitar un nuevo token de acceso utilizando un token de actualización. Contiene el token de actualización y el agente de usuario para validar la solicitud.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Token de atualização fornecido pelo cliente para obter um novo token de acesso. Deve ser uma string não vazia e é obrigatório para a solicitação de renovação do token.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Identificador do user agent do cliente, que deve ser uma string não vazia. Este campo é obrigatório e deve passar pela validação personalizada de user agent.
        /// </summary>
        [Required]
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
    }
}
