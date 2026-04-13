using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Dados para atualizar uma lista de compras.
    /// </summary>
    public class UpdateShoppingListRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Place { get; set; }
        public ShoppingListType Type { get; set; } = ShoppingListType.Grocery;
        public bool IsFinalized { get; set; }
        public ICollection<UpdateShoppingListItemRequest> Items { get; set; } = new List<UpdateShoppingListItemRequest>();
    }
}
