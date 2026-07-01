namespace eTasks_server.Models.DTOs.Goals.Responses
{
    /// <summary>
    /// Resposta da exclusão de uma meta.
    /// </summary>
    public class DeletedGoalResponse
    {
        /// <summary>
        /// Identificador único da meta excluída.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data e hora em que a meta foi excluída.
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
