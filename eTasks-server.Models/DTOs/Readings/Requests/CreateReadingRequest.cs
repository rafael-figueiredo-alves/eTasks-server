using eTasks_server.Models.Entities.Readings;

namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// Dados para criar um registro de leitura.
    /// </summary>
    public class CreateReadingRequest
    {
        public Guid? ClientGeneratedId { get; set; }

        /// <summary>
        /// Título da leitura. Campo obrigatório e não pode ser vazio.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Autores da leitura. Campo opcional.
        /// </summary>
        public string? Authors { get; set; }
        
        /// <summary>
        /// Assunto da leitura. Campo opcional.
        /// </summary>
        public string? Subject { get; set; }
        
        /// <summary>
        /// Resumo da leitura. Campo opcional.
        /// </summary>
        public string? Summary { get; set; }
        
        /// <summary>
        /// Opinião sobre a leitura. Campo opcional.
        /// </summary>
        public string? Opinion { get; set; }
        
        /// <summary>
        /// Avaliação da leitura. Campo opcional.
        /// </summary>
        public int? Rating { get; set; }
        
        /// <summary>
        /// Número total de páginas da leitura. Campo obrigatório.
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Página atual da leitura. Campo obrigatório.
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// Gênero da leitura. Campo opcional.
        /// </summary>
        public string? Genre { get; set; }
        
        /// <summary>
        /// Formato da leitura. Campo opcional.
        /// </summary>
        public ReadingFormat Format { get; set; } = ReadingFormat.Book;
        
        /// <summary>
        /// Status da leitura. Campo opcional.
        /// </summary>
        public ReadingStatus Status { get; set; } = ReadingStatus.ToRead;
        
        /// <summary>
        /// Data de início da leitura. Campo opcional.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// Data de término da leitura. Campo opcional.
        /// </summary>
        public DateTime? FinishedAt { get; set; }
    }
}
