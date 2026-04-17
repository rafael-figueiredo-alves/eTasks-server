using eTasks_server.Models.Entities.Readings;

namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// Filtros de consulta para leituras.
    /// </summary>
    public class ListReadingsRequest
    {
        /// <summary>
        /// Status da leitura (ex: "Lendo", "Lido", "Quero Ler"). Se nulo, retorna leituras de todos os status.
        /// </summary>
        public ReadingStatus? Status { get; set; }

        /// <summary>
        /// Formato da leitura (ex: "Livro", "Audiolivro", "E-book"). Se nulo, retorna leituras de todos os formatos.
        /// </summary>
        public ReadingFormat? Format { get; set; }

        /// <summary>
        /// Genero da leitura (ex: "Ficção", "Não-ficção", "Fantasia"). Se nulo, retorna leituras de todos os gêneros.
        /// </summary>
        public string? Genre { get; set; }

        /// <summary>
        /// Classificação da leitura (ex: 1 a 5 estrelas). Se nulo, retorna leituras de todas as classificações.
        /// </summary>
        public int? RatingFrom { get; set; }

        /// <summary>
        /// Classificação máxima da leitura (ex: 1 a 5 estrelas). Se nulo, retorna leituras de todas as classificações.
        /// </summary>
        public int? RatingTo { get; set; }

        /// <summary>
        /// Iniciada a leitura a partir de uma data específica. Se nulo, retorna leituras iniciadas em qualquer data.
        /// </summary>
        public DateTime? StartedFrom { get; set; }

        /// <summary>
        /// Finalizada a leitura até uma data específica. Se nulo, retorna leituras finalizadas em qualquer data.
        /// </summary>
        public DateTime? StartedTo { get; set; }

        /// <summary>
        /// Termo de pesquisa para filtrar leituras por título, autores ou resumo. Se nulo, retorna todas as leituras.
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
