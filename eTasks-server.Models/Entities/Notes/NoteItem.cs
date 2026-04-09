using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Notes
{
    /// <summary>
    /// Representa uma anotação livre do usuário.
    /// </summary>
    public class NoteItem : IEntityModelConfiguration<NoteItem>
    {
        /// <summary>
        /// Identificador único da anotação.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário dono da anotação.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Assunto principal da anotação.
        /// </summary>
        public string Subject { get; set; } = string.Empty;
        /// <summary>
        /// Conteúdo textual da anotação.
        /// </summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// Data de criação da anotação.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização da anotação.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuário dono da anotação.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de anotações.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NoteItem>()
                .ToTable("notes")
                .HasKey(x => x.Id);

            modelBuilder.Entity<NoteItem>()
                .HasIndex(x => x.UserUid);
        }
    }
}
