namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Resposta do push de sincronização de notas, contendo a hora do servidor e os resultados individuais para cada nota
    /// </summary>
    public class NotePushSyncResponse
    {
        /// <summary>
        /// Hora do servidor no momento da resposta, utilizada para sincronização de dados
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Resultados individuais para cada nota enviada no push de sincronização, indicando o status da operação
        /// </summary>
        public List<NotePushSyncItemResponse> Results { get; set; } = [];
    }
}
