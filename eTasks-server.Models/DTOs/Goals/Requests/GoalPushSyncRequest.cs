namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Representa a requisição de sincronização de metas, contendo uma lista de operações a serem realizadas.
    /// </summary>
    public class GoalPushSyncRequest
    {
        /// <summary>
        /// Obtém ou define a lista de operações de sincronização de metas.
        /// </summary>
        public List<GoalPushSyncItemRequest> Operations { get; set; } = [];
    }
}
