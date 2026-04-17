namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Resposta detalhada de uma anotacao.
    /// </summary>
    public class NoteDetailsResponse
    {
        /// <summary>
        /// Indica o identificador único da anotação.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Identificador do usuário associado à anotação.
        /// </summary>
        public Guid UserUid { get; set; }
        
        /// <summary>
        /// Assunto da anotação.
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        
        /// <summary>
        /// Conteúdo da anotação.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora de criação da anotação. Representa o momento em que a anotação foi criada no sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Data e hora da última atualização da anotação. Representa o momento em que a anotação foi modificada pela última vez no sistema.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
