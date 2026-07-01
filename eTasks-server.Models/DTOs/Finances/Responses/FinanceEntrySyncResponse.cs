namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resposta do endpoint de sincronização de lançamentos financeiros, contendo informações sobre os lançamentos atualizados e deletados, bem como o horário do servidor.
    /// </summary>
    public class FinanceEntrySyncResponse
    {
        /// <summary>
        /// Obtém ou define o horário do servidor no momento da sincronização.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Obtém ou define a lista de lançamentos financeiros que foram atualizados ou inseridos durante a sincronização.
        /// </summary>
        public List<FinanceEntryDetailsResponse> Upserts { get; set; } = [];

        /// <summary>
        /// Obtém ou define a lista de lançamentos financeiros que foram deletados durante a sincronização.
        /// </summary>
        public List<DeletedFinanceEntryResponse> Deleted { get; set; } = [];
    }
}
