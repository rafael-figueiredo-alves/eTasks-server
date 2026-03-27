using eTasks_server.Models.DataAnnotations;

namespace eTasks_server.Models.Users
{
    /// <summary>
    /// Entidade que representa um token de atualização (refresh token) para autenticação de usuários.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Identificador único do token de atualização, gerado usando o método CreateVersion7 para garantir unicidade e ordenação temporal.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário ao qual o token de atualização pertence, estabelecendo uma relação entre o token e o usuário correspondente.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Token de atualização gerado para autenticação do usuário, utilizado para obter novos tokens de acesso sem a necessidade de reautenticação completa.
        /// </summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// Identificador do dispositivo ou agente de usuário (user agent) que gerou o token de atualização, permitindo rastrear a origem do token e implementar medidas de segurança adicionais, como revogação de tokens específicos.
        /// </summary>
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
        /// <summary>
        /// Data em que o token de atualização expira, indicando o período de validade do token e permitindo a implementação de políticas de expiração para garantir a segurança da autenticação.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Identificador se o token de atualização foi revogado, indicando se o token foi invalidado antes do seu período de expiração, o que pode ocorrer em casos de comprometimento de segurança ou quando o usuário decide revogar o acesso a um dispositivo específico.
        /// </summary>
        public bool IsRevoked { get; set; } = false;
        /// <summary>
        /// Data de criação do token de atualização, registrando o momento em que o token foi gerado e permitindo rastrear a atividade de autenticação do usuário ao longo do tempo.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario associado ao token de atualização, estabelecendo uma relação de navegação entre o token e o usuário correspondente, permitindo acessar as informações do usuário a partir do token de atualização e vice-versa.
        /// </summary>
        // Navigation Property
        public User? User { get; set; }
    }
}
