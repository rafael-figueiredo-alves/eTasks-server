using eTasks_server.Models.Enums.Readings;

namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Item resumido da listagem de leituras.
    /// </summary>
    public class ReadingListItemResponse
    {
        /// <summary>
        /// Identificador único da leitura.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Título da leitura.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Autores da leitura.
        /// </summary>
        public string? Authors { get; set; }
        
        /// <summary>
        /// Assunto da leitura.
        /// </summary>
        public string? Subject { get; set; }
        
        /// <summary>
        /// Gênero da leitura.
        /// </summary>
        public string? Genre { get; set; }
        
        /// <summary>
        /// Avaliação da leitura.
        /// </summary>
        public int? Rating { get; set; }
        
        /// <summary>
        /// Total de páginas da leitura.
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Página atual da leitura.
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// Percentual de progresso da leitura.
        /// </summary>
        public decimal ProgressPercent { get; set; }
        
        /// <summary>
        /// Formato da leitura.
        /// </summary>
        public ReadingFormat Format { get; set; }
        
        /// <summary>
        /// Status da leitura.
        /// </summary>
        public ReadingStatus Status { get; set; }
        
        /// <summary>
        /// Data de início da leitura.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// Data de término da leitura.
        /// </summary>
        public DateTime? FinishedAt { get; set; }
    }
}
