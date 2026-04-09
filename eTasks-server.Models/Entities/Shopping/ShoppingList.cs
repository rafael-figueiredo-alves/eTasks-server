using eTasks_server.Models.Entities;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Shopping
{
    /// <summary>
    /// Representa uma lista de compras do usuário.
    /// </summary>
    public class ShoppingList : IEntityModelConfiguration<ShoppingList>
    {
        /// <summary>
        /// Identificador único da lista.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador do usuário dono da lista.
        /// </summary>
        public Guid UserUid { get; set; }
        /// <summary>
        /// Nome da lista de compras.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Nome do mercado, loja ou local associado.
        /// </summary>
        public string? StoreName { get; set; }
        /// <summary>
        /// Valor total estimado da compra.
        /// </summary>
        public decimal? EstimatedTotalAmount { get; set; }
        /// <summary>
        /// Valor total efetivamente gasto.
        /// </summary>
        public decimal? ActualTotalAmount { get; set; }
        /// <summary>
        /// Quantidade opcional de pontos ao concluir a lista.
        /// </summary>
        public int? RewardPoints { get; set; }
        /// <summary>
        /// Indica se a lista foi concluída.
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Data de conclusão da lista.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        /// <summary>
        /// Data de criação da lista.
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        /// <summary>
        /// Data da última atualização da lista.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Usuário dono da lista.
        /// </summary>
        public User? User { get; set; }
        /// <summary>
        /// Itens pertencentes à lista de compras.
        /// </summary>
        public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();

        /// <summary>
        /// Configura o mapeamento da entidade de listas de compras.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShoppingList>()
                .ToTable("shopping_lists")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ShoppingList>()
                .Property(x => x.EstimatedTotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShoppingList>()
                .Property(x => x.ActualTotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShoppingList>()
                .HasIndex(x => new { x.UserUid, x.IsCompleted });

            modelBuilder.Entity<ShoppingList>()
                .HasMany(x => x.Items)
                .WithOne(x => x.ShoppingList)
                .HasForeignKey(x => x.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
