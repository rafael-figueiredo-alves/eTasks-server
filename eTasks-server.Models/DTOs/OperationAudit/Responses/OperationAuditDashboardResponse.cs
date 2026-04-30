namespace eTasks_server.Models.DTOs.OperationAudit.Responses
{
    public class OperationAuditDashboardResponse
    {
        public bool MongoAuditEnabled { get; set; }
        public bool IsConfigured { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
        public long TotalEntries { get; set; }
        public long EntriesLast24Hours { get; set; }
        public long ErrorEntries { get; set; }
        public long AuthenticatedEntries { get; set; }
        public double AverageDurationMs { get; set; }
        public DateTime? LatestEntryAtUtc { get; set; }
        public IReadOnlyList<OperationAuditMetricResponse> StatusCodes { get; set; } = [];
        public IReadOnlyList<OperationAuditMetricResponse> Methods { get; set; } = [];
        public IReadOnlyList<OperationAuditMetricResponse> Resources { get; set; } = [];
        public IReadOnlyList<OperationAuditUsagePointResponse> UsageTrend { get; set; } = [];
    }
}
