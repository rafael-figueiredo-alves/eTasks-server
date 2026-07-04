using eTasks_server.Models.Enums.Common;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Dados de recorrencia usados em lancamentos financeiros.
    /// </summary>
    public class FinanceRecurrenceRequest
    {
        /// <summary>
        /// Tipo de recorrencia, se é mensal, semanal, anual ou nenhuma (recorrencia unica).
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

        /// <summary>
        /// Intervalo de recorrencia, por exemplo, se for mensal e o intervalo for 2, a recorrencia será a cada 2 meses.
        /// </summary>
        public int RecurrenceInterval { get; set; } = 1;

        /// <summary>
        /// Dia da semana em que a recorrencia ocorre, caso seja semanal. Pode ser uma combinação de dias, por exemplo, segunda e quarta.
        /// </summary>
        public WeekDays WeekDays { get; set; } = WeekDays.None;

        /// <summary>
        /// Dia do mês em que a recorrencia ocorre, caso seja mensal. Por exemplo, se for 15, a recorrencia ocorrerá no dia 15 de cada mês. Se for 31, a recorrencia ocorrerá no último dia de cada mês.
        /// </summary>
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Data em que a recorrencia termina. Se não for especificada, a recorrencia continuará indefinidamente.
        /// </summary>
        public DateTime? RecurrenceEndsOn { get; set; }
    }
}
