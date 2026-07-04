using eTasks_server.Models.Enums.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Dados para atualizar um item de compra.
    /// </summary>
    public class UpdateShoppingListItemRequest
    {
        /// <summary>
        /// Identificador do item de compra a ser atualizado.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Descrição do item de compra.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Unidade de medida do item de compra.
        /// </summary>
        public ShoppingItemUnit Unit { get; set; } = ShoppingItemUnit.Unit;

        /// <summary>
        /// Quantidade do item de compra.
        /// </summary>
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// Preço unitário do item de compra.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Indica se o item de compra foi concluído.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
