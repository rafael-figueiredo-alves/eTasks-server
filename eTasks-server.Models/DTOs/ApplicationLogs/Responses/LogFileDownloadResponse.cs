namespace eTasks_server.Models.DTOs.ApplicationLogs.Responses
{
    /// <summary>
    /// Resposta do download do arquivo de log.
    /// </summary>
    public class LogFileDownloadResponse
    {
        /// <summary>
        /// Nome do arquivo de log.
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de conteúdo do arquivo de log.
        /// </summary>
        public string ContentType { get; set; } = "text/plain";
        
        /// <summary>
        /// Conteúdo do arquivo de log.
        /// </summary>
        public byte[] Content { get; set; } = [];
    }
}
