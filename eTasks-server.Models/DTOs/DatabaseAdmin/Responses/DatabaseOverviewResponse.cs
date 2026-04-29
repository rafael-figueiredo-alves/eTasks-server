namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    public class DatabaseOverviewResponse
    {
        public string DatabaseName { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public long TotalRows { get; set; }
        public long DataLengthBytes { get; set; }
        public long IndexLengthBytes { get; set; }
        public DateTime GeneratedAt { get; set; }
        public IReadOnlyList<DatabaseTableSummaryResponse> Tables { get; set; } = [];

        public long TotalLengthBytes => DataLengthBytes + IndexLengthBytes;
    }
}
