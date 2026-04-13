namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Dados para atualizar uma anotacao.
    /// </summary>
    public class UpdateNoteRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
