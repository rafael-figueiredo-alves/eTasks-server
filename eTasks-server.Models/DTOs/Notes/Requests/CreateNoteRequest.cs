namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Dados para criar uma anotacao.
    /// </summary>
    public class CreateNoteRequest
    {
        /// <summary>
        /// Identificador gerado pelo cliente para operacao offline.
        /// </summary>
        public Guid? ClientGeneratedId { get; set; }

        /// <summary>
        /// Assunto da anotacao.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Conteudo da anotacao.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
