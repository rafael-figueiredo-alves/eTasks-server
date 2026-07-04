namespace eTasks_server.Models.Enums.Readings
{
    /// <summary>
    /// Representa o estado atual de uma leitura.
    /// </summary>
    public enum ReadingStatus
    {
        /// <summary>
        /// Leitura marcada para ler.
        /// </summary>
        ToRead = 0,
        /// <summary>
        /// Leitura em andamento.
        /// </summary>
        Reading = 1,
        /// <summary>
        /// Leitura concluída.
        /// </summary>
        Completed = 2
    }
}
