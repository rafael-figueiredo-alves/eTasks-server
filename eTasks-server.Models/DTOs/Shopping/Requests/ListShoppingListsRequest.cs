using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Filtros de consulta para listas de compras.
    /// </summary>
    public class ListShoppingListsRequest
    {
        public bool? IsFinalized { get; set; }
        public ShoppingListType? Type { get; set; }
        public string? Place { get; set; }
        public string? SearchTerm { get; set; }
    }
}
