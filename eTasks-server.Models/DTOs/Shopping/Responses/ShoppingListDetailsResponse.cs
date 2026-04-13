using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta detalhada de uma lista de compras.
    /// </summary>
    public class ShoppingListDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid UserUid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Place { get; set; }
        public ShoppingListType Type { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsFinalized { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ShoppingListItemResponse> Items { get; set; } = new List<ShoppingListItemResponse>();
    }
}
