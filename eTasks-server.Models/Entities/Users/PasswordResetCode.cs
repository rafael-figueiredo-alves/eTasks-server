using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Entidade de código para redefinição de senha
    /// </summary>
    public class PasswordResetCode : IEntityModelConfiguration<PasswordResetCode>
    {
        /// <summary>
        /// Identificador único
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Id do usuário
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Código de 6 digitos a ser usado para trocar/resetar senha
        /// </summary>
        [Length(6, 6, ErrorMessage = "O codigo de verificacao deve ter exatamente 6 digitos")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Data de expiração do código
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Marcador se o código já foi usado para invalisar tentativa de reuso
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Data de criação do código
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();


        /// <summary>
        /// Usuário atrelado
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Método de configuração
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PasswordResetCode>()
                                .ToTable("password_reset_codes")
                                .HasKey(x => x.Id);
        }
    }
}
