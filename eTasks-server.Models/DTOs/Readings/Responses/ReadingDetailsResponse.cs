using eTasks_server.Models.Enums.Readings;

namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Resposta detalhada de uma leitura.
    /// </summary>
    public class ReadingDetailsResponse
    {
        /// <summary>
        /// ID da leitura, representado por um GUID para garantir unicidade e segurança.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do usuário associado à leitura, representado por um GUID para garantir unicidade e segurança.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Titulo da leitura, obrigatório e não pode ser vazio. Representa o nome do livro ou material de leitura associado à leitura.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Autores da leitura, opcional. Representa os nomes dos autores do livro ou material de leitura associado à leitura. Pode ser uma string única ou uma lista de autores separados por vírgula.
        /// </summary>
        public string? Authors { get; set; }

        
        /// <summary>
        /// Assunto da leitura, opcional. Representa o tema ou tópico principal do livro ou material de leitura associado à leitura. Pode ser uma string que descreve o assunto de forma geral, como "Ficção Científica", "História", "Autoajuda", etc.
        /// </summary>
        public string? Subject { get; set; }
        
        /// <summary>
        /// Resumo da leitura, opcional. Representa uma breve descrição do conteúdo do livro ou material de leitura associado à leitura.
        /// </summary>
        public string? Summary { get; set; }
        
        /// <summary>
        /// Opinião sobre a leitura, opcional. Representa a opinião pessoal do usuário sobre o livro ou material de leitura associado à leitura.
        /// </summary>
        public string? Opinion { get; set; }
        
        /// <summary>
        /// Avaliação da leitura, opcional. Representa a nota ou avaliação do usuário sobre o livro ou material de leitura associado à leitura.
        /// </summary>
        public int? Rating { get; set; }
        
        /// <summary>
        /// Total de páginas da leitura, obrigatório. Representa o número total de páginas do livro ou material de leitura associado à leitura.
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Página atual da leitura, obrigatório. Representa a página em que o usuário está atualmente no livro ou material de leitura associado à leitura.
        /// </summary>
        public int CurrentPage { get; set; }
        
        /// <summary>
        /// Gênero da leitura, opcional. Representa o gênero ou categoria do livro ou material de leitura associado à leitura.
        /// </summary>
        public string? Genre { get; set; }
        
        /// <summary>
        /// Formato da leitura, obrigatório. Representa o formato do livro ou material de leitura associado à leitura, como físico, digital, audiobook, etc.
        /// </summary>
        public ReadingFormat Format { get; set; }
        
        /// <summary>
        /// Status da leitura, obrigatório. Representa o status atual da leitura, como "Para Ler", "Lendo", "Concluído", etc.
        /// </summary>
        public ReadingStatus Status { get; set; }
        
        /// <summary>
        /// Data de início da leitura, opcional. Representa a data em que o usuário começou a leitura do livro ou material de leitura associado à leitura.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        
        /// <summary>
        /// Data de término da leitura, opcional. Representa a data em que o usuário terminou a leitura do livro ou material de leitura associado à leitura.
        /// </summary>
        public DateTime? FinishedAt { get; set; }
        
        /// <summary>
        /// Data de criação do registro de leitura, obrigatório. Representa a data em que o registro de leitura foi criado.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Data de atualização do registro de leitura, opcional. Representa a data em que o registro de leitura foi atualizado pela última vez.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
