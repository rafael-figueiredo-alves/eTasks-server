namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Lote de mutacoes pendentes do cliente para sincronizacao.
    /// </summary>
    public class TaskPushSyncRequest
    {
        /// <summary>
        /// Operacoes pendentes da outbox local.
        /// </summary>
        public List<TaskPushSyncItemRequest> Operations { get; set; } = [];
    }
}
