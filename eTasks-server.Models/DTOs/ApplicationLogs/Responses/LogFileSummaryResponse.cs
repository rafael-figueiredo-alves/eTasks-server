namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    /// <summary>
    /// Resumo dos arquivos de log disponíveis no servidor.
    /// </summary>
    public class LogFileSummaryResponse
    {
        /// <summary>
        /// Nome do arquivo de log.
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Tamanho do arquivo de log em bytes.
        /// </summary>
        public long SizeBytes { get; set; }
        
        /// <summary>
        /// Data e hora da criação do arquivo de log.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Data e hora da última modificação do arquivo de log.
        /// </summary>
        public DateTime LastModifiedAt { get; set; }
    }
}
