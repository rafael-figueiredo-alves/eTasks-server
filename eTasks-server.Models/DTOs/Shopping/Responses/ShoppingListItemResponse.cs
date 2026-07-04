using eTasks_server.Models.Enums.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta de um item de compra.
    /// </summary>
    public class ShoppingListItemResponse
    {
        /// <summary>
        /// Identificador único do item de compra.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Descrição do item de compra.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Unidade de medida do item de compra.
        /// </summary>
        public ShoppingItemUnit Unit { get; set; }
        
        /// <summary>
        /// Quantidade do item de compra.
        /// </summary>
        public decimal Quantity { get; set; }
        
        /// <summary>
        /// Preço unitário do item de compra.
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Valor total do item de compra.
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Indica se o item de compra foi concluído.
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// Data de criação do item de compra.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
