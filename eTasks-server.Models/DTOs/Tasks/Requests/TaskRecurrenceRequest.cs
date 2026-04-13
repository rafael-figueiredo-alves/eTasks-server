using eTasks_server.Models.Entities.Common;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados de recorrencia usados no cadastro ou edicao de tarefas.
    /// </summary>
    public class TaskRecurrenceRequest
    {
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        public int Interval { get; set; } = 1;
        public WeekDays WeekDays { get; set; } = WeekDays.None;
        public int? DayOfMonth { get; set; }
        public int? MonthOfYear { get; set; }
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
