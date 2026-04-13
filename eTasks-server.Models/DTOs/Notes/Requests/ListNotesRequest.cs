namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Filtros de consulta para anotacoes.
    /// </summary>
    public class ListNotesRequest
    {
        public string? SearchTerm { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }
    }
}
