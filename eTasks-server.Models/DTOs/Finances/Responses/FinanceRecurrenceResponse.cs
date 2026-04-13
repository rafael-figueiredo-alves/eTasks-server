using eTasks_server.Models.Entities.Common;

namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resposta com os dados de recorrencia do lancamento financeiro.
    /// </summary>
    public class FinanceRecurrenceResponse
    {
        public RecurrenceType RecurrenceType { get; set; }
        public int RecurrenceInterval { get; set; }
        public WeekDays WeekDays { get; set; }
        public int? DayOfMonth { get; set; }
        public DateTime? RecurrenceEndsOn { get; set; }
    }
}
