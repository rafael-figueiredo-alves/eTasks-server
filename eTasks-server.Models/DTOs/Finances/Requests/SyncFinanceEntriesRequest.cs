namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Representa a solicitação de sincronização de entradas financeiras.
    /// </summary>
    public class SyncFinanceEntriesRequest
    {
        /// <summary>
        /// Obtém ou define a data a partir da qual as entradas financeiras devem ser sincronizadas.
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
