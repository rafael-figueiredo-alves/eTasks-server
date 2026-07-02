namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    /// <summary>
    /// Métrica de auditoría de operações, que representa un conteo de eventos agrupados por una etiqueta específica.
    /// </summary>
    public class OperationAuditMetricResponse
    {
        /// <summary>
        /// Etiqueta que representa la categoría o tipo de evento auditado.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Contador de eventos auditados que pertencem a uma etiqueta específica.
        /// </summary>
        public long Count { get; set; }
    }
}
