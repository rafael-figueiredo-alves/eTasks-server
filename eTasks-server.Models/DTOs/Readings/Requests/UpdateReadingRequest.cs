using eTasks_server.Models.Entities.Readings;

namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// Dados para atualizar um registro de leitura.
    /// </summary>
    public class UpdateReadingRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Authors { get; set; }
        public string? Subject { get; set; }
        public string? Summary { get; set; }
        public string? Opinion { get; set; }
        public int? Rating { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string? Genre { get; set; }
        public ReadingFormat Format { get; set; } = ReadingFormat.Book;
        public ReadingStatus Status { get; set; } = ReadingStatus.ToRead;
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
