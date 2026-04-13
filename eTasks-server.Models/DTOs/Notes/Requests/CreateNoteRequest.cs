namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Dados para criar uma anotacao.
    /// </summary>
    public class CreateNoteRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
