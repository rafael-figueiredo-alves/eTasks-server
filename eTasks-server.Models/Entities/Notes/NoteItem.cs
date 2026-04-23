using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notes
{
    /// <summary>
    /// Representa uma anotacao livre do usuario.
    /// </summary>
    public class NoteItem : IEntityModelConfiguration<NoteItem>
    {
        /// <summary>
        /// Identificador unico da anotacao.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Identificador do usuario dono da anotacao.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Assunto principal da anotacao.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Conteudo textual da anotacao.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Data de criacao da anotacao.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data da ultima atualizacao da anotacao.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica se a anotacao foi removida logicamente.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Data da remocao logica da anotacao.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Usuario dono da anotacao.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de anotacoes.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NoteItem>()
                .ToTable("notes")
                .HasKey(x => x.Id);

            modelBuilder.Entity<NoteItem>()
                .HasIndex(x => x.UserUid);

            modelBuilder.Entity<NoteItem>()
                .HasIndex(x => new { x.UserUid, x.IsDeleted });
        }
    }
}
