using eTasks_server.Models.Enums.Goals;
using eTasks_server.Models.Enums.Tasks;

namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Filtros de consulta para metas.
    /// </summary>
    public class ListGoalsRequest
    {
        /// <summary>
        /// Status da meta (Pendente, Em Progresso, Concluída, etc.).
        /// </summary>
        public GoalStatus? Status { get; set; }

        /// <summary>
        /// Tipo de meta (Curto Prazo, Longo Prazo, etc.).
        /// </summary>
        public GoalType? Type { get; set; }

        /// <summary>
        /// Prioridade da meta (Baixa, Média, Alta).
        /// </summary>
        public TaskPriority? Priority { get; set; }

        /// <summary>
        /// Indica se a meta é recompensada ou não. Se true, retorna apenas metas recompensadas; se false, retorna apenas metas não recompensadas; se null, retorna todas as metas independentemente de serem recompensadas ou não.
        /// </summary>
        public bool? OnlyRewarded { get; set; }

        /// <summary>
        /// Termo a buscar no título ou descrição da meta. Se fornecido, retorna apenas metas que contenham esse termo no título ou descrição. Se null ou vazio, retorna todas as metas independentemente do conteúdo do título ou descrição.
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
