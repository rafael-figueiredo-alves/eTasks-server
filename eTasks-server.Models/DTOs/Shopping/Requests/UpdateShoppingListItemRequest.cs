using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Dados para atualizar um item de compra.
    /// </summary>
    public class UpdateShoppingListItemRequest
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public ShoppingItemUnit Unit { get; set; } = ShoppingItemUnit.Unit;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public bool IsCompleted { get; set; }
    }
}
