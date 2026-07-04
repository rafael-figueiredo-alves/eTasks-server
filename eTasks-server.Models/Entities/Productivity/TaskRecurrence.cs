using eTasks_server.Models.Enums.Common;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Productivity
{
    /// <summary>
    /// Define a recorrência de uma tarefa base.
    /// </summary>
    public class TaskRecurrence : IEntityModelConfiguration<TaskRecurrence>
    {
        /// <summary>
        /// Identificador único da recorrência.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador da tarefa base.
        /// </summary>
        public Guid TaskItemId { get; set; }
        /// <summary>
        /// Tipo de recorrência configurado.
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Weekly;
        /// <summary>
        /// Intervalo numérico entre recorrências.
        /// </summary>
        public int Interval { get; set; } = 1;
        /// <summary>
        /// Dias da semana usados em recorrências semanais.
        /// </summary>
        public WeekDays WeekDays { get; set; } = WeekDays.None;
        /// <summary>
        /// Dia do mês usado em recorrências mensais.
        /// </summary>
        public int? DayOfMonth { get; set; }
        /// <summary>
        /// Mês do ano usado em recorrências anuais.
        /// </summary>
        public int? MonthOfYear { get; set; }
        /// <summary>
        /// Data de início da recorrência.
        /// </summary>
        public DateTime StartsOn { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data final opcional da recorrência.
        /// </summary>
        public DateTime? EndsOn { get; set; }
        /// <summary>
        /// Data em que a última geração automática ocorreu.
        /// </summary>
        public DateTime? LastGeneratedAt { get; set; }
        /// <summary>
        /// Indica se a recorrência está ativa.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tarefa vinculada à recorrência.
        /// </summary>
        public TaskItem? TaskItem { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de recorrência de tarefas.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskRecurrence>()
                .ToTable("task_recurrences")
                .HasKey(x => x.Id);

            modelBuilder.Entity<TaskRecurrence>()
                .Property(x => x.RecurrenceType)
                .HasConversion<int>();

            modelBuilder.Entity<TaskRecurrence>()
                .Property(x => x.WeekDays)
                .HasConversion<int>();

            modelBuilder.Entity<TaskRecurrence>()
                .HasIndex(x => x.TaskItemId)
                .IsUnique();
        }
    }
}
