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
        /// Nome do autor.
        /// </summary>
        public string? Author { get; set; }
        /// <summary>
        /// Descrição ou observações sobre a leitura.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Total de páginas do material.
        /// </summary>
        public int? TotalPages { get; set; }
        /// <summary>
        /// Página atual de progresso.
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// Avaliação dada pelo usuário.
        /// </summary>
        public int? Rating { get; set; }
        /// <summary>
        /// Pontuação opcional concedida na conclusão.
        /// </summary>
        public int? RewardPoints { get; set; }
        /// <summary>
        /// Estado atual da leitura.
        /// </summary>
        public ReadingStatus Status { get; set; } = ReadingStatus.Planned;
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
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<ReadingItem>()
                .HasIndex(x => new { x.UserUid, x.Status });
        }
    }
}
