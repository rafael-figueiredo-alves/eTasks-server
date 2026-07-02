namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    /// <summary>
    /// Representa entrada de registro de auditoría de operação.
    /// </summary>
    public class OperationAuditLogEntryResponse
    {
        /// <summary>
        /// Identificador único da entrada de registro de auditoría de operação.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Data e hora em que a entrada de registro de auditoría de operação foi criada (em UTC).
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Identificador de rastreamento associado à entrada de registro de auditoría de operação.
        /// </summary>
        public string TraceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Método HTTP da solicitação associada à entrada de registro de auditoría de operação.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Caminho da solicitação associada à entrada de registro de auditoría de operação.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Query string da solicitação associada à entrada de registro de auditoría de operação.
        /// </summary>
        public string? QueryString { get; set; }

        /// <summary>
        /// Nome do endpoint associado à entrada de registro de auditoría de operação.
        /// </summary>
        public string? EndpointName { get; set; }

        /// <summary>
        /// Nome do recurso associado à entrada de registro de auditoría de operação.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Código de status HTTP da resposta associada à entrada de registro de auditoría de operação.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Duração da solicitação associada à entrada de registro de auditoría de operação, em milissegundos.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Identificador único do usuário associado à entrada de registro de auditoría de operação.
        /// </summary>
        public Guid? UserUid { get; set; }

        /// <summary>
        /// Indica se o usuário associado à entrada de registro de auditoría de operação está autenticado.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// User agent da solicitação associada à entrada de registro de auditoría de operação.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Endereço IP da solicitação associada à entrada de registro de auditoría de operação.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Mensagem de erro associada à entrada de registro de auditoría de operação, se houver.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
