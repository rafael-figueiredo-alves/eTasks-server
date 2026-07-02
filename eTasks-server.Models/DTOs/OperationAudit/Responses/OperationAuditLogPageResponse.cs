namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    /// <summary>
    /// Paginação de registros de auditoria de operações.
    /// </summary>
    public class OperationAuditLogPageResponse
    {
        /// <summary>
        /// Lista de registros de auditoria de operações.
        /// </summary>
        public IReadOnlyList<OperationAuditLogEntryResponse> Items { get; set; } = [];

        /// <summary>
        /// Número total de registros de auditoria de operações disponíveis.
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// Número da página atual (baseado em 1).
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Número de registros de auditoria de operações por página.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Número total de páginas disponíveis com base no TotalItems e PageSize.
        /// </summary>
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    }
}
