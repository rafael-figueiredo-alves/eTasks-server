using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta de um item de compra.
    /// </summary>
    public class ShoppingListItemResponse
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public ShoppingItemUnit Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
