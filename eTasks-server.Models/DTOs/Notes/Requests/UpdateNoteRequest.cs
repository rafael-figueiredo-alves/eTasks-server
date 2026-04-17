namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Dados para atualizar uma anotacao.
    /// </summary>
    public class UpdateNoteRequest
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
