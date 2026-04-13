using eTasks_server.Models.Entities.Readings;

namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Resposta detalhada de uma leitura.
    /// </summary>
    public class ReadingDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid UserUid { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Subject { get; set; }
        public string? Summary { get; set; }
        public string? Opinion { get; set; }
        public int? Rating { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Genre { get; set; }
        public ReadingFormat Format { get; set; }
        public ReadingStatus Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
