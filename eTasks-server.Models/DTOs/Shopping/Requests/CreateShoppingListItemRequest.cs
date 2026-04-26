using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Dados para adicionar um item em uma lista de compras.
    /// </summary>
    public class CreateShoppingListItemRequest
    {
        public Guid? ClientGeneratedId { get; set; }

        /// <summary>
        /// Descrição do item a ser adicionado. Exemplo: "Leite", "Pão", "Ovos", etc.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Unidade de medida para a quantidade do item. Exemplo: "Unidade", "Litro", "Quilo", etc. O valor padrão é "Unidade".
        /// </summary>
        public ShoppingItemUnit Unit { get; set; } = ShoppingItemUnit.Unit;
        
        /// <summary>
        /// Quantidade do item a ser adicionado. O valor padrão é 1.
        /// </summary>
        public decimal Quantity { get; set; } = 1;
        
        /// <summary>
        /// Preço unitário do item.
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Indica se o item foi comprado.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
