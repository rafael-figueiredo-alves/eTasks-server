namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Resposta a atualização de leituras via push sync.
    /// </summary>
    public class ReadingPushSyncResponse
    {
        /// <summary>
        /// Horário do servidor no momento da resposta.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Resultados da atualização de leituras, incluindo o status de cada item.
        /// </summary>
        public List<ReadingPushSyncItemResponse> Results { get; set; } = [];
    }
}
