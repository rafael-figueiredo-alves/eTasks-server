using eTasks_server.Models.Entities.Common;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Dados de recorrencia usados em lancamentos financeiros.
    /// </summary>
    public class FinanceRecurrenceRequest
    {
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        public int RecurrenceInterval { get; set; } = 1;
        public WeekDays WeekDays { get; set; } = WeekDays.None;
        public int? DayOfMonth { get; set; }
        public DateTime? RecurrenceEndsOn { get; set; }
    }
}
