using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Classe que representa um código de reativação de conta para um usuário. Este código é gerado quando um usuário solicita a reativação de sua conta e é usado para validar a solicitação. O código tem uma data de expiração e pode ser marcado como usado para evitar reutilização.
    /// </summary>
    public class AccountReactivationCode : IEntityModelConfiguration<AccountReactivationCode>
    {
        /// <summary>
        /// Identificador único do código de reativação, gerado usando o método CreateVersion7 para garantir unicidade e ordenação temporal.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador único do usuário associado a este código de reativação. Este campo é usado para vincular o código ao usuário que solicitou a reativação da conta.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// O código de reativação da conta, gerado aleatoriamente e usado para validar a solicitação de reativação.
        /// </summary>
        [MaxLength(128)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// A data e hora em que o código de reativação expira. Após essa data, o código não será mais válido para reativar a conta do usuário.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Indica se o código de reativação já foi usado para reativar a conta do usuário. Se for verdadeiro, o código não pode ser reutilizado para reativar a conta novamente.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// A data e hora em que o código de reativação foi usado para reativar a conta do usuário. Este campo é preenchido quando o código é marcado como usado e pode ser útil para auditoria e rastreamento de atividades relacionadas à reativação de contas.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// A data e hora em que o código de reativação foi criado. Este campo é preenchido automaticamente com a data e hora atual no momento da criação do código e pode ser útil para auditoria e rastreamento de atividades relacionadas à criação de códigos de reativação.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// O usuário associado a este código de reativação.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Método de configuração do modelo para a entidade AccountReactivationCode. Este método é chamado pelo Entity Framework Core para configurar as propriedades e relacionamentos da entidade no banco de dados. Ele define a tabela, as chaves primárias, os índices e os relacionamentos necessários para a entidade AccountReactivationCode.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountReactivationCode>()
                .ToTable("account_reactivation_codes")
                .HasKey(x => x.Id);

            modelBuilder.Entity<AccountReactivationCode>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<AccountReactivationCode>()
                .HasIndex(x => new { x.UserUid, x.IsUsed, x.ExpiresAt });
        }
    }
}
