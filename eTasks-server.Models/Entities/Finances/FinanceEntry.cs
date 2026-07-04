using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Finances;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Finances
{
    /// <summary>
    /// Representa um lancamento financeiro do usuario.
    /// </summary>
    public class FinanceEntry : IEntityModelConfiguration<FinanceEntry>
    {
        /// <summary>
        /// Identificador unico do lancamento.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do usuario dono do lancamento.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Titulo principal do lancamento.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descricao complementar do lancamento.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Categoria financeira do lancamento.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Pessoa ou entidade relacionada ao lancamento.
        /// </summary>
        public string? Counterparty { get; set; }

        /// <summary>
        /// Tipo do lancamento financeiro.
        /// </summary>
        public FinanceEntryType EntryType { get; set; } = FinanceEntryType.Debit;

        /// <summary>
        /// Forma de pagamento usada no lancamento.
        /// </summary>
        public FinancePaymentMethod PaymentMethod { get; set; } = FinancePaymentMethod.Other;

        /// <summary>
        /// Valor monetario do lancamento.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Data em que o lancamento ocorre.
        /// </summary>
        public DateTime OccursOn { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Indica se o lancamento foi pago ou efetivado.
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Data de efetivacao do lancamento.
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Indica se o lancamento possui recorrencia.
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Data de criacao do lancamento.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data da ultima atualizacao do lancamento.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Identifica se o lancamento foi deletado (exclusao logica).
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Identifica a data de exclusao do lancamento, caso tenha sido deletado.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Usuario dono do lancamento.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configuracao de recorrencia associada ao lancamento.
        /// </summary>
        public FinanceRecurrence? Recurrence { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de financas.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinanceEntry>()
                .ToTable("finance_entries")
                .HasKey(x => x.Id);

            modelBuilder.Entity<FinanceEntry>()
                .Property(x => x.EntryType)
                .HasConversion<int>();

            modelBuilder.Entity<FinanceEntry>()
                .Property(x => x.PaymentMethod)
                .HasConversion<int>();

            modelBuilder.Entity<FinanceEntry>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceEntry>()
                .HasIndex(x => new { x.UserUid, x.OccursOn });

            modelBuilder.Entity<FinanceEntry>()
                .HasOne(x => x.Recurrence)
                .WithOne(x => x.FinanceEntry)
                .HasForeignKey<FinanceRecurrence>(x => x.FinanceEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
