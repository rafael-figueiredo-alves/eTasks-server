using eTasks_server.Models.Entities.Productivity;

namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Filtros de consulta para a listagem de tarefas.
    /// </summary>
    public class ListTasksRequest
    {
        /// <summary>
        /// Filtrar peloa data de referência. Este campo é opcional e pode ser usado para filtrar as tarefas com base em uma data específica. As tarefas que tiverem uma data igual ou posterior a esta data serão incluídas nos resultados da consulta.
        /// </summary>
        public DateTime? ReferenceDate { get; set; }
        
        /// <summary>
        /// Filtrar pelo intervalo de datas. Este campo é opcional e pode ser usado para filtrar as tarefas que ocorrerem dentro de um intervalo de datas específico.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Data final do intervalo de datas. Este campo é opcional e pode ser usado para filtrar as tarefas que ocorrerem dentro de um intervalo de datas específico. As tarefas que tiverem uma data igual ou anterior a esta data serão incluídas nos resultados da consulta.
        /// </summary>
        public DateTime? DateTo { get; set; }
        
        /// <summary>
        /// Filtrar pelo status de conclusão da tarefa. Este campo é opcional e pode ser usado para filtrar as tarefas com base em seu status de conclusão.
        /// </summary>
        public bool? IsCompleted { get; set; }
        
        /// <summary>
        /// Filtrar pela prioridade da tarefa. Este campo é opcional e pode ser usado para filtrar as tarefas com base em sua prioridade.
        /// </summary>
        public TaskPriority? Priority { get; set; }
        
        /// <summary>
        /// Filtrar pelo termo de pesquisa. Este campo é opcional e pode ser usado para filtrar as tarefas com base em um termo de pesquisa específico.
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
