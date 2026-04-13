using eTasks_server.Models.Entities.Common;

namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resposta com os dados de recorrencia de uma tarefa.
    /// </summary>
    public class TaskRecurrenceResponse
    {
        public Guid Id { get; set; }
        public RecurrenceType RecurrenceType { get; set; }
        public int Interval { get; set; }
        public WeekDays WeekDays { get; set; }
        public int? DayOfMonth { get; set; }
        public int? MonthOfYear { get; set; }
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public DateTime? LastGeneratedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
