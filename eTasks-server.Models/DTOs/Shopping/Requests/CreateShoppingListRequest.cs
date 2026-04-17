using eTasks_server.Models.Entities.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Dados para criar uma lista de compras.
    /// </summary>
    public class CreateShoppingListRequest
    {
        /// <summary>
        /// Nome da lista de compras. Exemplo: "Lista de Compras Semanal", "Lista de Compras para Festa", etc.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Local onde a lista de compras será utilizada. Exemplo: "Supermercado", "Padaria", etc.
        /// </summary>
        public string? Place { get; set; }
        
        /// <summary>
        /// Tipo da lista de compras. Exemplo: "Grocery", "Party", etc.
        /// </summary>
        public ShoppingListType Type { get; set; } = ShoppingListType.Grocery;
        
        /// <summary>
        /// Indica se a lista de compras foi finalizada.
        /// </summary>
        public bool IsFinalized { get; set; }
        
        /// <summary>
        /// Itens da lista de compras.
        /// </summary>
        public ICollection<CreateShoppingListItemRequest> Items { get; set; } = new List<CreateShoppingListItemRequest>();
    }
}
