using eTasks_server.Models.Enums.Common;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Finances
{
    /// <summary>
    /// Representa a configuracao de recorrencia de um lancamento financeiro base.
    /// </summary>
    public class FinanceRecurrence : IEntityModelConfiguration<FinanceRecurrence>
    {
        /// <summary>
        /// Identificador unico da recorrencia.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do lancamento financeiro base.
        /// </summary>
        public Guid FinanceEntryId { get; set; }

        /// <summary>
        /// Tipo de recorrencia configurado.
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Monthly;

        /// <summary>
        /// Intervalo numerico entre recorrencias.
        /// </summary>
        public int Interval { get; set; } = 1;

        /// <summary>
        /// Dias da semana usados em recorrencias semanais.
        /// </summary>
        public WeekDays WeekDays { get; set; } = WeekDays.None;

        /// <summary>
        /// Dia do mes usado em recorrencias mensais.
        /// </summary>
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Data final opcional da recorrencia.
        /// </summary>
        public DateTime? EndsOn { get; set; }

        /// <summary>
        /// Lancamento financeiro vinculado a recorrencia.
        /// </summary>
        public FinanceEntry? FinanceEntry { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de recorrencia de financas.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinanceRecurrence>()
                .ToTable("finance_recurrences")
                .HasKey(x => x.Id);

            modelBuilder.Entity<FinanceRecurrence>()
                .Property(x => x.RecurrenceType)
                .HasConversion<int>();

            modelBuilder.Entity<FinanceRecurrence>()
                .Property(x => x.WeekDays)
                .HasConversion<int>();

            modelBuilder.Entity<FinanceRecurrence>()
                .HasIndex(x => x.FinanceEntryId)
                .IsUnique();
        }
    }
}
