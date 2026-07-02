namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Resposta ao excluir um registro de leitura.
    /// </summary>
    public class DeletedReadingResponse
    {
        /// <summary>
        /// Identificador único do registro de leitura excluído.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data e hora em que o registro de leitura foi excluído.
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
