namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    public class DatabaseTableSummaryResponse
    {
        public string Name { get; set; } = string.Empty;
        public long Rows { get; set; }
        public long DataLengthBytes { get; set; }
        public long IndexLengthBytes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public long TotalLengthBytes => DataLengthBytes + IndexLengthBytes;
    }
}
