namespace eTasks_server.Models.Enums.Tasks
{
    /// <summary>
    /// Status de processamento de uma mutacao no push sync.
    /// </summary>
    public enum TaskPushSyncItemStatus
    {
        /// <summary>
        /// aplicadas com sucesso
        /// </summary>
        Applied = 1,

        /// <summary>
        /// ocorreu conflito
        /// </summary>
        Conflict = 2,

        /// <summary>
        /// erro de validação
        /// </summary>
        ValidationError = 3,

        /// <summary>
        /// Não encontradas
        /// </summary>
        NotFound = 4,

        /// <summary>
        /// Falharam
        /// </summary>
        Failed = 5
    }
}
