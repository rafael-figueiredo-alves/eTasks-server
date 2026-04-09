using eTasks_server.Models.Entities;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Gamification
{
    /// <summary>
    /// Centraliza as regras padrão de pontuação por origem de bônus.
    /// </summary>
    public class BonusPointRule : IEntityModelConfiguration<BonusPointRule>
    {
        /// <summary>
        /// Identificador único da regra.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Origem do ponto coberta pela regra.
        /// </summary>
        public BonusPointSource Source { get; set; } = BonusPointSource.TaskCompletion;
        /// <summary>
        /// Nome amigável da regra.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Descrição resumida da finalidade da regra.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Quantidade padrão de pontos atribuída.
        /// </summary>
        public int DefaultPoints { get; set; }
        /// <summary>
        /// Indica se a pontuação pode ser sobrescrita em lançamentos específicos.
        /// </summary>
        public bool AllowCustomPoints { get; set; } = true;
        /// <summary>
        /// Indica se a regra está ativa para uso.
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Data de criação da regra.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização da regra.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de regras de bônus.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BonusPointRule>()
                .ToTable("bonus_point_rules")
                .HasKey(x => x.Id);

            modelBuilder.Entity<BonusPointRule>()
                .Property(x => x.Source)
                .HasConversion<int>();

            modelBuilder.Entity<BonusPointRule>()
                .HasIndex(x => x.Source)
                .IsUnique();
        }
    }
}
