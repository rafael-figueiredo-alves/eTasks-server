using eTasks_server.Models.Enums.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta detalhada de uma lista de compras.
    /// </summary>
    public class ShoppingListDetailsResponse
    {
        /// <summary>
        /// Identificador único da lista de compras.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Identificador único do usuário dono da lista de compras.
        /// </summary>
        public Guid UserUid { get; set; }
        
        /// <summary>
        /// Nome da lista de compras.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Local onde a lista de compras será utilizada.
        /// </summary>
        public string? Place { get; set; }
        
        /// <summary>
        /// Tipo da lista de compras.
        /// </summary>
        public ShoppingListType Type { get; set; }
        
        /// <summary>
        /// Total de itens na lista de compras.
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Valor total da lista de compras.
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Indica se a lista de compras foi finalizada.
        /// </summary>
        public bool IsFinalized { get; set; }
        
        /// <summary>
        /// Data de criação da lista de compras.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Data da última atualização da lista de compras.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Itens da lista de compras.
        /// </summary>
        public ICollection<ShoppingListItemResponse> Items { get; set; } = new List<ShoppingListItemResponse>();
    }
}
