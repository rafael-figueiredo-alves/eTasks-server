namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resposta a exclusão de uma entrada financeira.
    /// </summary>
    public class DeletedFinanceEntryResponse
    {
        /// <summary>
        /// Identificador único da entrada financeira excluída.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data e hora em que a entrada financeira foi excluída.
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
