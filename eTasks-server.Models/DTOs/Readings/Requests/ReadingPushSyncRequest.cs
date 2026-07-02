namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// DTO que representa uma solicitação de sincronização de push de leitura.
    /// </summary>
    public class ReadingPushSyncRequest
    {
        /// <summary>
        /// Lista de operações de push de leitura a serem sincronizadas.
        /// </summary>
        public List<ReadingPushSyncItemRequest> Operations { get; set; } = [];
    }
}
