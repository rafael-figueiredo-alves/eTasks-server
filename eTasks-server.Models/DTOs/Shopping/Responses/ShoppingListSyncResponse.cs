namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta da sincronização das listas de compras
    /// </summary>
    public class ShoppingListSyncResponse
    {
        /// <summary>
        /// Horário do servidor
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Inserções e alterações da lista de compras
        /// </summary>
        public List<ShoppingListDetailsResponse> Upserts { get; set; } = [];

        /// <summary>
        /// Lista dos registros excluídos
        /// </summary>
        public List<DeletedShoppingListResponse> Deleted { get; set; } = [];
    }
}
