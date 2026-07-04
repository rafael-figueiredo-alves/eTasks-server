using eTasks_server.Models.Enums.Common;

namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resposta com os dados de recorrencia do lancamento financeiro.
    /// </summary>
    public class FinanceRecurrenceResponse
    {
        /// <summary>
        /// Tipo de recorrencia do lancamento financeiro.
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; }

        /// <summary>
        /// Intervalo da recorrencia do lancamento financeiro. Ex: se a recorrencia for mensal e o intervalo for 2, o lancamento financeiro sera repetido a cada 2 meses.
        /// </summary>
        public int RecurrenceInterval { get; set; }

        /// <summary>
        /// Dias da semana em que o lancamento financeiro sera repetido. Ex: se a recorrencia for semanal e os dias da semana forem segunda e quarta, o lancamento financeiro sera repetido toda segunda e quarta-feira.
        /// </summary>
        public WeekDays WeekDays { get; set; }

        /// <summary>
        /// Dia do mes em que o lancamento financeiro sera repetido. Ex: se a recorrencia for mensal e o dia do mes for 15, o lancamento financeiro sera repetido todo dia 15 de cada mes.
        /// </summary>
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Quando a recorrencia do lancamento financeiro termina. Ex: se a recorrencia for mensal e a data de termino for 31/12/2024, o lancamento financeiro sera repetido todo dia 15 de cada mes ate 31/12/2024.
        /// </summary>
        public DateTime? RecurrenceEndsOn { get; set; }
    }
}
