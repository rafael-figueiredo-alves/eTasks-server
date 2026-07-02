namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Resposta do servidor para a sincronização de notas, incluindo o tempo do servidor, notas atualizadas e notas deletadas.
    /// </summary>
    public class NoteSyncResponse
    {
        /// <summary>
        /// Obtém ou define o tempo atual do servidor, usado para sincronização de notas.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Inserções ou atualizações de notas que foram feitas no servidor desde a última sincronização.
        /// </summary>
        public List<NoteDetailsResponse> Upserts { get; set; } = [];

        /// <summary>
        /// Registros de notas que foram deletadas no servidor desde a última sincronização.     
        /// </summary>
        public List<DeletedNoteResponse> Deleted { get; set; } = [];
    }
}
