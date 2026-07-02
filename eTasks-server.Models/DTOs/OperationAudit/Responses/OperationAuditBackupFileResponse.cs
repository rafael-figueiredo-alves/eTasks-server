namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    /// <summary>
    /// Representa a resposta de um arquivo de backup de auditoria de operações.
    /// </summary>
    public class OperationAuditBackupFileResponse
    {
        /// <summary>
        /// Obtém ou define o nome do arquivo de backup de auditoria de operações.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Contém o tipo de conteúdo do arquivo de backup de auditoria de operações.
        /// </summary>
        public string ContentType { get; set; } = "application/x-ndjson";

        /// <summary>
        /// Obtém ou define o conteúdo do arquivo de backup de auditoria de operações em formato de array de bytes.
        /// </summary>
        public byte[] Content { get; set; } = [];
    }
}
