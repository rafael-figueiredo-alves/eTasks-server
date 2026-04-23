namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resposta do push sync da outbox de tarefas.
    /// </summary>
    public class TaskPushSyncResponse
    {
        /// <summary>
        /// Horario do servidor no momento do processamento.
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Resultado por mutacao enviada pelo cliente.
        /// </summary>
        public List<TaskPushSyncItemResponse> Results { get; set; } = [];
    }
}
