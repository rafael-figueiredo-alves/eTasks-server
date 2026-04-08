using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Classe de log de logins
    /// </summary>
    public class LoginLog : IEntityModelConfiguration<LoginLog>
    {
        /// <summary>
        /// Identificador único de registro
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// ID do usuário
        /// </summary>
        public Guid? UserUid { get; set; }

        /// <summary>
        /// Status da tentativa de login
        /// </summary>

        [AllowedValues(["Success", "Failed", "Blocked"], ErrorMessage = "Os únicos status aceitos são 'Success', 'Failed' e/ou 'Blocked'")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de IP que fez tentativa de acesso
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Agente identificador do cliente
        /// </summary>
        [AllowedUserAgent]
        public string? UserAgent { get; set; }

        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Usuário relacionado
        /// </summary>
        [JsonIgnore]
        public User? User { get; set; }

        /// <summary>
        /// Método de configuração
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginLog>()
                                .ToTable("login_logs")
                                .HasKey(x => x.Id);
        }
    }
}
