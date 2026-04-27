using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Classe controladora dos tokens de atualização (refresh tokens) para autenticação e autorização de usuários. Ela contém propriedades para armazenar o token, a data de expiração, o agente do usuário e outras informações relevantes. A classe também implementa a interface IEntityModelConfiguration para configurar o modelo usando o Fluent API do Entity Framework Core.
    /// </summary>
    public class RefreshToken : IEntityModelConfiguration<RefreshToken>
    {
        /// <summary>
        /// Identificação do registro de token de atualização, gerada usando o método CreateVersion7 para garantir a unicidade e a ordenação temporal dos tokens.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificação do usuário associado ao token de atualização, permitindo a vinculação entre o token e o usuário para fins de autenticação e autorização.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Token de atualização (refresh token) utilizado para obter um novo token de acesso (access token) quando o token de acesso atual expira. Ele é gerado e armazenado de forma segura para garantir a integridade e a confidencialidade do processo de autenticação.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Agente do cliente (user agent) associado ao token de atualização, que pode ser utilizado para identificar o dispositivo ou a aplicação que está utilizando o token. Essa informação pode ser útil para fins de segurança e monitoramento, permitindo detectar atividades suspeitas ou não autorizadas.
        /// </summary>
        [AllowedUserAgent]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Quando expirará o token de atualização, indicando o momento em que ele se tornará inválido e não poderá mais ser utilizado para obter um novo token de acesso. Essa propriedade é fundamental para garantir a segurança do processo de autenticação, limitando a validade dos tokens e reduzindo o risco de uso indevido ou comprometimento da conta do usuário.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Identifica se o token de atualização foi revogado, ou seja, se ele foi invalidado antes do seu período de expiração. Essa propriedade é importante para garantir a segurança do processo de autenticação, permitindo que os tokens sejam revogados em caso de suspeita de comprometimento ou uso indevido, mesmo que ainda estejam dentro do período de validade.
        /// </summary>
        public bool IsRevoked { get; set; }
        
        /// <summary>
        /// Data de criação do Token de Atualização
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();


        /// <summary>
        /// Associação do usuário do Token
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Método para setar configurações adicionais do modelo RefreshToken usando o Fluent API do Entity Framework Core.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens").HasKey(x => x.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => new { x.IsRevoked, x.ExpiresAt });
        }
    }
}
