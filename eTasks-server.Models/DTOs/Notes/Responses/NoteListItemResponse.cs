namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Item resumido da listagem de anotacoes.
    /// </summary>
    public class NoteListItemResponse
    {
        public Guid Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
