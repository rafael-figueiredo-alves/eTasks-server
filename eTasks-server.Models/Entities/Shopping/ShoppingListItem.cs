using eTasks_server.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Shopping
{
    /// <summary>
    /// Representa um item individual de uma lista de compras.
    /// </summary>
    public class ShoppingListItem : IEntityModelConfiguration<ShoppingListItem>
    {
        /// <summary>
        /// Identificador único do item.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();
        /// <summary>
        /// Identificador da lista de compras.
        /// </summary>
        public Guid ShoppingListId { get; set; }
        /// <summary>
        /// Nome do item a comprar.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Marca do item.
        /// </summary>
        public string? Brand { get; set; }
        /// <summary>
        /// Quantidade desejada do item.
        /// </summary>
        public decimal Quantity { get; set; } = 1;
        /// <summary>
        /// Preço unitário informado para o item.
        /// </summary>
        public decimal? UnitPrice { get; set; }
        /// <summary>
        /// Indica se o item já foi comprado.
        /// </summary>
        public bool IsCompleted { get; set; }
        /// <summary>
        /// Observações adicionais sobre o item.
        /// </summary>
        public string? Notes { get; set; }
        /// <summary>
        /// Data de criação do item.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lista de compras à qual o item pertence.
        /// </summary>
        public ShoppingList? ShoppingList { get; set; }

        /// <summary>
        /// Configura o mapeamento da entidade de itens de compra.
        /// </summary>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShoppingListItem>()
                .ToTable("shopping_list_items")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ShoppingListItem>()
                .Property(x => x.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShoppingListItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShoppingListItem>()
                .HasIndex(x => new { x.ShoppingListId, x.IsCompleted });
        }
    }
}
