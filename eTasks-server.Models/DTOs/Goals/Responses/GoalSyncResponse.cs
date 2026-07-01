namespace eTasks_server.Models.DTOs.Goals.Responses
{
    /// <summary>
    /// Resposta da sincronização de metas, contendo informações sobre metas atualizadas e deletadas, bem como o timestamp do servidor.
    /// </summary>
    public class GoalSyncResponse
    {
        /// <summary>
        /// Timestamp do servidor no momento da resposta, utilizado para controle de sincronização e versionamento.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Inserções ou atualizações de metas, contendo detalhes das metas sincronizadas.
        /// </summary>
        public List<GoalDetailsResponse> Upserts { get; set; } = [];

        /// <summary>
        /// Deleções de metas, contendo informações sobre as metas que foram removidas.
        /// </summary>
        public List<DeletedGoalResponse> Deleted { get; set; } = [];
    }
}
