namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Representa a resposta da sincronização de lançamentos financeiros.
    /// </summary>
    public class FinanceEntryPushSyncResponse
    {
        /// <summary>
        /// Obtém ou define o horário do servidor no momento da resposta.
        /// </summary>
        public DateTime ServerTime { get; set; }
        
        /// <summary>
        /// Obtém ou define a lista de resultados da sincronização.
        /// </summary>
        public List<FinanceEntryPushSyncItemResponse> Results { get; set; } = [];
    }
}
