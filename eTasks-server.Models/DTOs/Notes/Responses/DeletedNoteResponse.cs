namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Resposta ao excluir uma nota, contendo o ID da nota excluída e a data de exclusão.
    /// </summary>
    public class DeletedNoteResponse
    {
        /// <summary>
        /// Identificador único da nota que foi excluída.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data e hora em que a nota foi excluída.
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
