using eTasks_server.Models.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace eTasks_server.Models.Users
{
    /// <summary>
    /// Classe para registrar os logs de login dos usuários, incluindo informações como status do login, endereço IP, user agent e timestamp.
    /// </summary>
    public class LoginLog
    {
        /// <summary>
        /// Identificador único do log de login, utilizando GUID versão 7 para garantir unicidade e ordenação temporal.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7(); // Requer .NET 10 ou custom
        /// <summary>
        /// Identificador do usuário associado ao log de login, utilizando GUID para garantir unicidade e segurança. Pode ser nulo para tentativas de login anônimas ou falhas antes da identificação do usuário.
        /// </summary>
        public Guid? UserUid { get; set; }
        /// <summary>
        /// Guarda o status do login, indicando se foi bem-sucedido ou falhou, permitindo análises de segurança e monitoramento de atividades suspeitas.
        /// </summary>
        /// <example>
        /// Valores aceitos: Failed, Success, Blocked.
        /// </example>
        [AllowedValues(["Success", "Failed", "Blocked"], ErrorMessage = "Os únicos status aceitos são 'Success', 'Failed' e/ou 'Blocked'")]
        public string Status { get; set; } = string.Empty; // e.g., "Success", "Failed"
        /// <summary>
        /// Guarda o endereço IP do usuário no momento do login, permitindo análises de segurança e monitoramento de atividades suspeitas. Pode ser nulo para tentativas de login anônimas ou falhas antes da identificação do usuário.
        /// </summary>      
        public string? IpAddress { get; set; }
        /// <summary>
        /// Guarda o user agent do navegador ou dispositivo utilizado no momento do login, permitindo análises de segurança e monitoramento de atividades suspeitas. Pode ser nulo para tentativas de login anônimas ou falhas antes da identificação do usuário.
        /// </summary>
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
        /// <summary>
        /// Data de criação do log de login, utilizando o horário UTC para garantir consistência em diferentes fusos horários e facilitar análises temporais. O valor padrão é a data e hora atual no momento da criação do log.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Informações do Usuário associado ao log de login, utilizando uma relação de chave estrangeira com a entidade User. Esta propriedade é ignorada na serialização JSON para evitar exposição de dados sensíveis e reduzir o tamanho da resposta em APIs. Pode ser nula para tentativas de login anônimas ou falhas antes da identificação do usuário.
        /// </summary>
        [JsonIgnore]
        public User? User { get; set; }
    }
}
