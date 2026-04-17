namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Dados para criar uma anotacao.
    /// </summary>
    public class CreateNoteRequest
    {
        /// <summary>
        /// Assunto da anotacao.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo da anotacao.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
