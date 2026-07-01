namespace eTasks_server.Models.DTOs.Goals.Responses
{
    /// <summary>
    /// Retorno da resposta do endpoint de sincronização de metas (GoalPushSync).
    /// </summary>
    public class GoalPushSyncResponse
    {
        /// <summary>
        /// Timestamp do servidor no momento da resposta, utilizado para controle de sincronização e versionamento.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Lista de resultados da sincronização de metas, contendo o status de cada operação e detalhes das metas sincronizadas.
        /// </summary>
        public List<GoalPushSyncItemResponse> Results { get; set; } = [];
    }
}
