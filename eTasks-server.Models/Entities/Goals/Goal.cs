using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Goals
{
    /// <summary>
    /// Representa uma meta ou objetivo definido pelo usuário.
    /// </summary>
    public class Goal : IEntityModelConfiguration<Goal>
    {
        /// <summary>
        /// Identificador único da meta.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário dono da meta.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Resumo curto da meta.
        /// </summary>
        public string Summary { get; set; } = string.Empty;
        /// <summary>
        /// Descrição da meta.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Tipo principal da meta.
        /// </summary>
        public GoalType Type { get; set; } = GoalType.Personal;
        /// <summary>
        /// Prioridade da meta.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        /// <summary>
        /// Pontos opcionais concedidos ao concluir a meta.
        /// </summary>
        public int? RewardPoints { get; set; }
        /// <summary>
        /// Estado atual da meta.
        /// </summary>
        public GoalStatus Status { get; set; } = GoalStatus.Active;
        /// <summary>
        /// Data de criação da meta.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização da meta.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuário dono da meta.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de metas.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>()
                .ToTable("goals")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Goal>()
                .Property(x => x.Status)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .Property(x => x.Type)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .Property(x => x.Priority)
                .HasConversion<int>();

            modelBuilder.Entity<Goal>()
                .HasIndex(x => new { x.UserUid, x.Status });
        }
    }
}
