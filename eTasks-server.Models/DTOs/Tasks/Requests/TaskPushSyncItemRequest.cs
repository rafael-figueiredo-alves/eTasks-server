namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Item de mutacao enviado pelo cliente no push sync.
    /// </summary>
    public class TaskPushSyncItemRequest
    {
        /// <summary>
        /// Identificador unico da mutacao no cliente.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da operacao a ser aplicada.
        /// </summary>
        public TaskPushOperationType Operation { get; set; }

        /// <summary>
        /// Identificador da tarefa alvo quando aplicavel.
        /// </summary>
        public Guid? TaskId { get; set; }

        /// <summary>
        /// ETag esperado pelo cliente para controle de concorrencia otimista.
        /// </summary>
        public string? ExpectedEtag { get; set; }

        /// <summary>
        /// Payload para operacoes de criacao.
        /// </summary>
        public CreateTaskRequest? Create { get; set; }

        /// <summary>
        /// Payload para operacoes de atualizacao.
        /// </summary>
        public UpdateTaskRequest? Update { get; set; }

        /// <summary>
        /// Payload para operacoes de conclusao.
        /// </summary>
        public SetTaskCompletionRequest? Completion { get; set; }
    }
}
