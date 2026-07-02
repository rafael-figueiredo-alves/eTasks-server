namespace eTasks_server.Models.DTOs.OperationAudit.Requests
{
    /// <summary>
    /// Classe da requesição de consulta de logs de auditoria de operações.
    /// </summary>
    public class OperationAuditLogQueryRequest
    {
        /// <summary>
        /// Obtém ou define o índice da página para a consulta de logs de auditoria de operações.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Obtém ou define o tamanho da página para a consulta de logs de auditoria de operações.
        /// </summary>
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Obtém ou define o termo de pesquisa para filtrar os logs de auditoria de operações.
        /// </summary>
        public string Search { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o método HTTP para filtrar os logs de auditoria de operações.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o código de status HTTP para filtrar os logs de auditoria de operações.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Obtém ou define o nome do recurso para filtrar os logs de auditoria de operações.
        /// </summary>
        public string ResourceName { get; set; } = string.Empty;
    }
}
