using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Filtros de consulta para a listagem de tarefas.
    /// </summary>
    public class ListTasksRequest
    {
        /// <summary>
        /// Filtra pela data de referencia.
        /// </summary>
        public DateTime? ReferenceDate { get; set; }

        /// <summary>
        /// Data inicial do intervalo.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Data final do intervalo.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Filtra pelo status de conclusao.
        /// </summary>
        public bool? IsCompleted { get; set; }

        /// <summary>
        /// Filtra pela prioridade da tarefa.
        /// </summary>
        public TaskPriority? Priority { get; set; }

        /// <summary>
        /// Termo livre para pesquisa em resumo e anotacoes.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Indica se tarefas recorrentes devem ser consideradas na consulta.
        /// Quando verdadeiro, o sistema pode materializar as ocorrencias do periodo consultado.
        /// </summary>
        public bool IncludeRecurring { get; set; } = true;
    }
}
