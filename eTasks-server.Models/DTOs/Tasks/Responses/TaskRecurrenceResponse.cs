using eTasks_server.Models.Enums.Common;

namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resposta com os dados de recorrencia de uma tarefa.
    /// </summary>
    public class TaskRecurrenceResponse
    {
        /// <summary>
        /// Identificador único da recorrência da tarefa.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Tipo de recorrência da tarefa.
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; }
        
        /// <summary>
        /// Intervalo da recorrência.
        /// </summary>
        public int Interval { get; set; }
        
        /// <summary>
        /// Dias da semana em que a tarefa se repete.
        /// </summary>
        public WeekDays WeekDays { get; set; }
        
        /// <summary>
        /// Dia do mês em que a tarefa se repete, se aplicável.
        /// </summary>
        public int? DayOfMonth { get; set; }
        
        /// <summary>
        /// Mês do ano em que a tarefa se repete, se aplicável.
        /// </summary>
        public int? MonthOfYear { get; set; }
        
        /// <summary>
        /// Data de início da recorrência.
        /// </summary>
        public DateTime StartsOn { get; set; }
        
        /// <summary>
        /// Data de término da recorrência, se aplicável.
        /// </summary>
        public DateTime? EndsOn { get; set; }
        
        /// <summary>
        /// Data e hora da última geração da recorrência.
        /// </summary>
        public DateTime? LastGeneratedAt { get; set; }
        
        /// <summary>
        /// Indica se a recorrência está ativa.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
