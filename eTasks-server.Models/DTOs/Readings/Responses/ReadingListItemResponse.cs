using eTasks_server.Models.Entities.Readings;

namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Item resumido da listagem de leituras.
    /// </summary>
    public class ReadingListItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Subject { get; set; }
        public string? Genre { get; set; }
        public int? Rating { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public decimal ProgressPercent { get; set; }
        public ReadingFormat Format { get; set; }
        public ReadingStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
