namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resposta de sincronizacao incremental de tarefas.
    /// </summary>
    public class TaskSyncResponse
    {
        /// <summary>
        /// Horario do servidor no momento da resposta.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Tarefas novas ou alteradas desde o cursor informado.
        /// </summary>
        public List<TaskDetailsResponse> Upserts { get; set; } = [];

        /// <summary>
        /// Tombstones de tarefas removidas logicamente desde o cursor informado.
        /// </summary>
        public List<DeletedTaskResponse> Deleted { get; set; } = [];
    }
}
