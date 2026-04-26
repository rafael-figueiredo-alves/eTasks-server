using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Common;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Finances
{
    /// <summary>
    /// Representa um lançamento financeiro do usuário.
    /// </summary>
    public class FinanceEntry : IEntityModelConfiguration<FinanceEntry>
    {
        /// <summary>
        /// Identificador único do lançamento.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário dono do lançamento.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Título principal do lançamento.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Descrição complementar do lançamento.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Categoria financeira do lançamento.
        /// </summary>
        public string? Category { get; set; }
        /// <summary>
        /// Pessoa ou entidade relacionada ao lançamento.
        /// </summary>
        public string? Counterparty { get; set; }
        /// <summary>
        /// Tipo do lançamento financeiro.
        /// </summary>
        public FinanceEntryType EntryType { get; set; } = FinanceEntryType.Debit;
        /// <summary>
        /// Forma de pagamento usada no lançamento.
        /// </summary>
        public FinancePaymentMethod PaymentMethod { get; set; } = FinancePaymentMethod.Other;
        /// <summary>
        /// Valor monetário do lançamento.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Data em que o lançamento ocorre.
        /// </summary>
        public DateTime OccursOn { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Indica se o lançamento foi pago ou efetivado.
        /// </summary>
        public bool IsPaid { get; set; }
        /// <summary>
        /// Data de efetivação do lançamento.
        /// </summary>
        public DateTime? PaidAt { get; set; }
        /// <summary>
        /// Indica se o lançamento possui recorrência.
        /// </summary>
        public bool IsRecurring { get; set; }
        /// <summary>
        /// Tipo de recorrência do lançamento.
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        /// <summary>
        /// Intervalo usado na recorrência do lançamento.
        /// </summary>
        public int RecurrenceInterval { get; set; } = 1;
        /// <summary>
        /// Dias da semana usados na recorrência, quando aplicável.
        /// </summary>
        public WeekDays WeekDays { get; set; } = WeekDays.None;
        /// <summary>
        /// Dia do mês usado na recorrência, quando aplicável.
        /// </summary>
        public int? DayOfMonth { get; set; }
        /// <summary>
        /// Data final opcional da recorrência.
        /// </summary>
        public DateTime? RecurrenceEndsOn { get; set; }
        /// <summary>
        /// Data de criação do lançamento.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização do lançamento.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Usuário dono do lançamento.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de finanças.
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
                .Property(x => x.RecurrenceType)
                .HasConversion<int>();

            modelBuilder.Entity<FinanceEntry>()
                .Property(x => x.WeekDays)
                .HasConversion<int>();

            modelBuilder.Entity<FinanceEntry>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FinanceEntry>()
                .HasIndex(x => new { x.UserUid, x.OccursOn });
        }
    }
}
