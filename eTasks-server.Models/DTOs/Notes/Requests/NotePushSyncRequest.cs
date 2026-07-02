namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Representa a requisição de sincronização de notas, contendo uma lista de operações a serem realizadas.
    /// </summary>
    public class NotePushSyncRequest
    {
        /// <summary>
        /// Obtém ou define a lista de operações de sincronização de notas.
        /// </summary>
        public List<NotePushSyncItemRequest> Operations { get; set; } = [];
    }
}
