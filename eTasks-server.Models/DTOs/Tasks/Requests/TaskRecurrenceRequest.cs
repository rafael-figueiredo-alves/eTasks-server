using eTasks_server.Models.Enums.Common;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados de recorrencia usados no cadastro ou edicao de tarefas.
    /// </summary>
    public class TaskRecurrenceRequest
    {
        /// <summary>
        /// Tipo de recorrencia da tarefa. Se for None, os outros campos de recorrencia serao ignorados.
        /// </summary>
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

        /// <summary>
        /// Intervalo de repeticao da tarefa. Por exemplo, se o tipo de recorrencia for Semanal e o intervalo for 2, a tarefa se repetira a cada 2 semanas. O valor padrao é 1, ou seja, a tarefa se repetira a cada semana, mes ou ano, dependendo do tipo de recorrencia selecionado.
        /// </summary>
        public int Interval { get; set; } = 1;

        /// <summary>
        /// Dia da semana em que a tarefa se repetira, caso o tipo de recorrencia seja Semanal. Pode ser uma combinacao de dias da semana, por exemplo, Segunda e Quarta. Se o tipo de recorrencia for diferente de Semanal, este campo sera ignorado.
        /// </summary>
        public WeekDays WeekDays { get; set; } = WeekDays.None;

        /// <summary>
        /// Dia do mes em que a tarefa se repetira, caso o tipo de recorrencia seja Mensal. Por exemplo, se o valor for 15, a tarefa se repetira no dia 15 de cada mes. Se o tipo de recorrencia for diferente de Mensal, este campo sera ignorado.
        /// </summary>
        public int? DayOfMonth { get; set; }

        /// <summary>
        /// Mes do ano em que a tarefa se repetira, caso o tipo de recorrencia seja Anual. Por exemplo, se o valor for 12, a tarefa se repetira no mes de dezembro de cada ano. Se o tipo de recorrencia for diferente de Anual, este campo sera ignorado.
        /// </summary>
        public int? MonthOfYear { get; set; }

        /// <summary>
        /// Quando a tarefa comecara a se repetir. A partir desta data, a tarefa sera criada de acordo com o tipo de recorrencia e os outros campos relacionados. Se a data de inicio for no passado, a tarefa sera criada imediatamente e depois se repetira conforme o tipo de recorrencia selecionado.
        /// </summary>
        public DateTime StartsOn { get; set; }

        /// <summary>
        /// Quando a tarefa deixara de se repetir. Se este campo for nulo, a tarefa se repetira indefinidamente. Se a data de fim for no passado, a tarefa nao sera criada. Se a data de fim for antes da data de inicio, a tarefa nao sera criada.
        /// </summary>
        public DateTime? EndsOn { get; set; }

        /// <summary>
        /// Se a tarefa recorrente esta ativa ou nao. Se este campo for falso, a tarefa recorrente nao sera criada e nenhuma tarefa relacionada a ela sera criada. Se este campo for verdadeiro, a tarefa recorrente sera criada e as tarefas relacionadas a ela serao criadas de acordo com o tipo de recorrencia e os outros campos relacionados.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
