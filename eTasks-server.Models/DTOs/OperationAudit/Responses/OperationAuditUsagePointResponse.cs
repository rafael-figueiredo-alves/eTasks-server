namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    /// <summary>
    /// Resposta de auditoria de operação para ponto de uso.
    /// </summary>
    public class OperationAuditUsagePointResponse
    {
        /// <summary>
        /// Data e hora de início do bucket em UTC.
        /// </summary>
        public DateTime BucketStartUtc { get; set; }
        
        /// <summary>
        /// Etiqueta que representa a categoria ou tipo de evento auditado.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Número total de eventos auditados no bucket.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Número de eventos auditados que não foram processados com sucesso.
        /// </summary>
        public long ErrorCount { get; set; }
    }
}
