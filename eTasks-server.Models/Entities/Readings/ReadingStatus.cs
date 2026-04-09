namespace eTasks_server.Models.Entities.Readings
{
    /// <summary>
    /// Representa o estado atual de uma leitura.
    /// </summary>
    public enum ReadingStatus
    {
        /// <summary>
        /// Leitura planejada.
        /// </summary>
        Planned = 0,
        /// <summary>
        /// Leitura em andamento.
        /// </summary>
        InProgress = 1,
        /// <summary>
        /// Leitura concluída.
        /// </summary>
        Completed = 2,
        /// <summary>
        /// Leitura pausada.
        /// </summary>
        Paused = 3,
        /// <summary>
        /// Leitura cancelada.
        /// </summary>
        Cancelled = 4
    }
}
