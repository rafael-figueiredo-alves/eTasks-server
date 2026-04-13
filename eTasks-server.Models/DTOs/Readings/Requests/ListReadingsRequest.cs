using eTasks_server.Models.Entities.Readings;

namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// Filtros de consulta para leituras.
    /// </summary>
    public class ListReadingsRequest
    {
        public ReadingStatus? Status { get; set; }
        public ReadingFormat? Format { get; set; }
        public string? Genre { get; set; }
        public int? RatingFrom { get; set; }
        public int? RatingTo { get; set; }
        public DateTime? StartedFrom { get; set; }
        public DateTime? StartedTo { get; set; }
        public string? SearchTerm { get; set; }
    }
}
