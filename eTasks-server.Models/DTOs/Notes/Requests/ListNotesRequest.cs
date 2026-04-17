namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Filtros de consulta para anotacoes.
    /// </summary>
    public class ListNotesRequest
    {
        /// <summary>
        /// Termo a buscar no assunto ou conteúdo da anotacao.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Data de criação ou atualização da anotacao para filtrar os resultados.
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Data de criação ou atualização da anotacao para filtrar os resultados.
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Data de atualização da anotacao para filtrar os resultados.
        /// </summary>
        public DateTime? UpdatedFrom { get; set; }

        /// <summary>
        /// Data de atualização da anotacao para filtrar os resultados.
        /// </summary>
        public DateTime? UpdatedTo { get; set; }
    }
}
