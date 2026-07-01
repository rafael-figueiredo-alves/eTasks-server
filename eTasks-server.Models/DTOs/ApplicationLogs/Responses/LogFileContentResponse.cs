namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    /// <summary>
    /// Resposta do conteúdo de um arquivo de log.
    /// </summary>
    public class LogFileContentResponse
    {
        /// <summary>
        /// Nome do arquivo de log.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo do arquivo de log.
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Tamanho do arquivo de log em bytes.
        /// </summary>
        public long SizeBytes { get; set; }
        
        /// <summary>
        /// Data e hora da última modificação do arquivo de log.
        /// </summary>
        public DateTime LastModifiedAt { get; set; }
    }
}
