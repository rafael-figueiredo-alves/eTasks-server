namespace eTasks_server.Models.Enums.Goals
{
    /// <summary>
    /// Representa o estado atual de uma meta.
    /// </summary>
    public enum GoalStatus
    {
        /// <summary>
        /// Meta ativa.
        /// </summary>
        Active = 0,
        /// <summary>
        /// Meta concluída.
        /// </summary>
        Completed = 1,
        /// <summary>
        /// Meta cancelada.
        /// </summary>
        Cancelled = 2,
        /// <summary>
        /// Meta arquivada.
        /// </summary>
        Archived = 3
    }
}
