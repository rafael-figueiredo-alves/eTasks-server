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
        /// Local onde a compra sera realizada.
        /// </summary>
        public string? Place { get; set; }
        /// <summary>
        /// Tipo principal da lista.
        /// </summary>
        public ShoppingListType Type { get; set; } = ShoppingListType.Grocery;
        /// <summary>
        /// Quantidade total de itens da lista.
        /// </summary>
        public int TotalItems { get; set; }
        /// <summary>
        /// Valor total da lista, calculado a partir dos itens.
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Indica se a lista foi finalizada.
        /// </summary>
        public bool IsFinalized { get; set; }
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
                .Property(x => x.Type)
                .HasConversion<int>();

            modelBuilder.Entity<ShoppingList>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShoppingList>()
                .HasIndex(x => new { x.UserUid, x.IsFinalized });

            modelBuilder.Entity<ShoppingList>()
                .HasMany(x => x.Items)
                .WithOne(x => x.ShoppingList)
                .HasForeignKey(x => x.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
