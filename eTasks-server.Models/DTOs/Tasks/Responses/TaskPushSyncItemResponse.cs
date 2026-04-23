namespace eTasks_server.Models.DTOs.Tasks.Responses
{
    /// <summary>
    /// Resultado do processamento de uma mutacao no push sync.
    /// </summary>
    public class TaskPushSyncItemResponse
    {
        /// <summary>
        /// Identificador da mutacao no cliente.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Status do processamento.
        /// </summary>
        public TaskPushSyncItemStatus Status { get; set; }

        /// <summary>
        /// Codigo de erro amigavel para o cliente quando houver falha.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Mensagem de erro amigavel quando houver falha.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Tarefa resultante da operacao, quando houver.
        /// </summary>
        public TaskDetailsResponse? Task { get; set; }

        /// <summary>
        /// Tombstone resultante da operacao de exclusao, quando houver.
        /// </summary>
        public DeletedTaskResponse? Deleted { get; set; }

        /// <summary>
        /// ETag mais recente do recurso, quando houver.
        /// </summary>
        public string? ServerEtag { get; set; }
    }
}
