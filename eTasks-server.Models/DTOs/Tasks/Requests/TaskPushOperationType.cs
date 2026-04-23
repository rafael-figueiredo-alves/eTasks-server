namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Tipos de operacao suportados no push sync de tarefas.
    /// </summary>
    public enum TaskPushOperationType
    {
        Create = 1,
        Update = 2,
        SetCompletion = 3,
        Delete = 4
    }
}
