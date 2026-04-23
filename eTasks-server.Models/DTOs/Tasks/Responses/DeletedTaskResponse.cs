namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Tombstone de tarefa removida logicamente.
    /// </summary>
    public class DeletedTaskResponse
    {
        /// <summary>
        /// Identificador da tarefa removida.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador da tarefa de origem em ocorrencias recorrentes.
        /// </summary>
        public Guid? GeneratedFromTaskId { get; set; }

        /// <summary>
        /// Data da remocao logica.
        /// </summary>
        public DateTime DeletedAt { get; set; }
    }
}
