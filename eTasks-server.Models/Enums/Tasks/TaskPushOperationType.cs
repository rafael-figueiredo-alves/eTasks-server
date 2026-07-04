namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Tipos de operacao suportados no push sync de tarefas.
    /// </summary>
    public enum TaskPushOperationType
    {
        /// <summary>
        /// Criar
        /// </summary>
        Create = 1,

        /// <summary>
        /// Atualizar
        /// </summary>
        Update = 2,

        /// <summary>
        /// Marcar tarefa concluída
        /// </summary>
        SetCompletion = 3,

        /// <summary>
        /// Excluir
        /// </summary>
        Delete = 4
    }
}
