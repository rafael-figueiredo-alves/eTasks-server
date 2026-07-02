namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    /// <summary>
    /// Representa a resposta do painel de auditoria de operações.
    /// </summary>
    public class OperationAuditDashboardResponse
    {
        /// <summary>
        /// Obtém ou define um valor que indica se a auditoria de operações no MongoDB está habilitada.
        /// </summary>
        public bool MongoAuditEnabled { get; set; }

        /// <summary>
        /// Obtém ou define um valor que indica se a auditoria de operações está configurada corretamente.
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Obtém ou define o nome do banco de dados onde os registros de auditoria de operações estão armazenados.
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o nome da coleção onde os registros de auditoria de operações estão armazenados.
        /// </summary>
        public string CollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o número total de registros de auditoria de operações.
        /// </summary>
        public long TotalEntries { get; set; }

        /// <summary>
        /// Obtém ou define o número de registros de auditoria de operações nos últimos 24 horas.
        /// </summary>
        public long EntriesLast24Hours { get; set; }

        /// <summary>
        /// Obtém ou define o número de registros de auditoria de operações com status de erro.
        /// </summary>
        public long ErrorEntries { get; set; }

        /// <summary>
        /// Obtém ou define o número de registros de auditoria de operações autenticados.
        /// </summary>
        public long AuthenticatedEntries { get; set; }

        /// <summary>
        /// Obtém ou define a duração média das operações auditadas em milissegundos.
        /// </summary>
        public double AverageDurationMs { get; set; }

        /// <summary>
        /// Obtém ou define a data e hora do último registro de auditoria de operações.
        /// </summary>
        public DateTime? LatestEntryAtUtc { get; set; }

        /// <summary>
        /// Obtém ou define a lista de métricas de auditoria de operações agrupadas por códigos de status.
        /// </summary>
        public IReadOnlyList<OperationAuditMetricResponse> StatusCodes { get; set; } = [];

        /// <summary>
        /// Obtém ou define a lista de métricas de auditoria de operações agrupadas por métodos HTTP.
        /// </summary>
        public IReadOnlyList<OperationAuditMetricResponse> Methods { get; set; } = [];

        /// <summary>
        /// Obtém ou define a lista de métricas de auditoria de operações agrupadas por recursos.
        /// </summary>
        public IReadOnlyList<OperationAuditMetricResponse> Resources { get; set; } = [];

        /// <summary>
        /// Obtém ou define a lista de métricas de auditoria de operações agrupadas por pontos de uso.
        /// </summary>
        public IReadOnlyList<OperationAuditUsagePointResponse> UsageTrend { get; set; } = [];
    }
}
