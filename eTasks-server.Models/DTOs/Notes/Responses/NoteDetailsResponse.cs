namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Resposta detalhada de uma anotacao.
    /// </summary>
    public class NoteDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid UserUid { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
