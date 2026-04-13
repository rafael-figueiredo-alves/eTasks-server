using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Item resumido da listagem de compras.
    /// </summary>
    public class ShoppingListListItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Place { get; set; }
        public ShoppingListType Type { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsFinalized { get; set; }
        public int CompletedItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
