namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    /// <summary>
    /// Entrada de log em tempo real.
    /// </summary>
    public class RealtimeLogEntryResponse
    {
        /// <summary>
        /// Timestamp do log.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Nível do log (por exemplo, "Information", "Warning", "Error").
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem do log.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exceção associada ao log, se houver.
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// Fonte do log (por exemplo, o nome da classe ou do componente que gerou o log).
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }
}
