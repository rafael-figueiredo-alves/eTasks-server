namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Requisição de sincronização de lançamentos financeiros.
    /// </summary>
    public class FinanceEntryPushSyncRequest
    {
        /// <summary>
        /// Operações de sincronização de lançamentos financeiros a serem realizadas.
        /// </summary>
        public List<FinanceEntryPushSyncItemRequest> Operations { get; set; } = [];
    }
}
