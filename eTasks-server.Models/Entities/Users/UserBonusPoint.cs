using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Users
{
    /// <summary>
    /// Representa um lançamento de pontos recebido pelo usuário.
    /// </summary>
    public class UserBonusPoint : IEntityModelConfiguration<UserBonusPoint>
    {
        /// <summary>
        /// Identificador único do lançamento de pontos.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário que recebeu os pontos.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Quantidade de pontos atribuída.
        /// </summary>
        public int Points { get; set; }
        /// <summary>
        /// Origem da pontuação.
        /// </summary>
        public BonusPointSource Source { get; set; } = BonusPointSource.ManualAdjustment;
        /// <summary>
        /// Descrição livre do lançamento.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Referência opcional para o recurso que originou os pontos.
        /// </summary>
        public Guid? SourceReferenceId { get; set; }
        /// <summary>
        /// Data de criação do lançamento.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Usuário associado ao lançamento de pontos.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de pontos do usuário.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserBonusPoint>().ToTable("user_bonus_points").HasKey(x => x.Id);

            modelBuilder.Entity<UserBonusPoint>()
                .Property(x => x.Source)
                .HasConversion<int>();
        }
    }
}
