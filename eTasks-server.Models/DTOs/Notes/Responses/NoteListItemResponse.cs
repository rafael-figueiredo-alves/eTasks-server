namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Item resumido da listagem de anotacoes.
    /// </summary>
    public class NoteListItemResponse
    {
        /// <summary>
        /// Identificador da anotacao.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Assunto da anotacao.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Pré-visualização do conteúdo da anotacao.
        /// </summary>
        public string Preview { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora de criação da anotacao.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data e hora da última atualização da anotacao.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
