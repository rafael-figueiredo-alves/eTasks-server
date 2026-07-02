namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Resposta do servidor para a sincronização de leituras, incluindo o tempo do servidor, leituras atualizadas e leituras deletadas.
    /// </summary>
    public class ReadingSyncResponse
    {
        /// <summary>
        /// Obtém ou define o tempo atual do servidor no momento da resposta.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Inserções ou atualizações de leituras que foram processadas pelo servidor.
        /// </summary>
        public List<ReadingDetailsResponse> Upserts { get; set; } = [];

        /// <summary>
        /// Deleções de leituras que foram processadas pelo servidor.
        /// </summary>
        public List<DeletedReadingResponse> Deleted { get; set; } = [];
    }
}
