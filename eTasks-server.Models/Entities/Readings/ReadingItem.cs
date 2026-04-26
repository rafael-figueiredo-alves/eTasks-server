using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Readings
{
    /// <summary>
    /// Representa um livro, jornal ou material de leitura do usuário.
    /// </summary>
    public class ReadingItem : IEntityModelConfiguration<ReadingItem>
    {
        /// <summary>
        /// Identificador único da leitura.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário dono da leitura.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Título do material.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Nome do autor ou autores.
        /// </summary>
        public string? Authors { get; set; }
        /// <summary>
        /// Assunto principal da leitura.
        /// </summary>
        public string? Subject { get; set; }
        /// <summary>
        /// Resumo do material lido.
        /// </summary>
        public string? Summary { get; set; }
        /// <summary>
        /// Opiniao pessoal sobre a leitura.
        /// </summary>
        public string? Opinion { get; set; }
        /// <summary>
        /// Avaliacao de zero a cinco estrelas.
        /// </summary>
        public int? Rating { get; set; }
        /// <summary>
        /// Total de paginas do material.
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// Pagina atual de progresso.
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// Genero principal da leitura.
        /// </summary>
        public string? Genre { get; set; }
        /// <summary>
        /// Tipo de publicacao registrada.
        /// </summary>
        public ReadingFormat Format { get; set; } = ReadingFormat.Book;
        /// <summary>
        /// Estado atual da leitura.
        /// </summary>
        public ReadingStatus Status { get; set; } = ReadingStatus.ToRead;
        /// <summary>
        /// Data de início da leitura.
        /// </summary>
        public DateTime? StartedAt { get; set; }
        /// <summary>
        /// Data de conclusão da leitura.
        /// </summary>
        public DateTime? FinishedAt { get; set; }
        /// <summary>
        /// Data de criação do registro.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização do registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Usuário dono da leitura.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de leituras.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReadingItem>()
                .ToTable("reading_items")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ReadingItem>()
                .Property(x => x.Format)
                .HasConversion<int>();

            modelBuilder.Entity<ReadingItem>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<ReadingItem>()
                .HasIndex(x => new { x.UserUid, x.Status });
        }
    }
}
