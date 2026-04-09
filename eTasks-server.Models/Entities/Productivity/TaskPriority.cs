namespace eTasks_server.Models.Entities.Productivity
{
    /// <summary>
    /// Define o nível de prioridade da tarefa.
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// Prioridade baixa.
        /// </summary>
        Low = 0,
        /// <summary>
        /// Prioridade média.
        /// </summary>
        Medium = 1,
        /// <summary>
        /// Prioridade alta.
        /// </summary>
        High = 2,
        /// <summary>
        /// Prioridade crítica.
        /// </summary>
        Critical = 3
    }
}
