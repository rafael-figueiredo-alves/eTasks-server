namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta da sincronização da lista de compras
    /// </summary>
    public class ShoppingListPushSyncResponse
    {
        /// <summary>
        /// Horário do servidor
        /// </summary>
        public DateTime ServerTime { get; set; }

        /// <summary>
        /// Itens retornados
        /// </summary>
        public List<ShoppingListPushSyncItemResponse> Results { get; set; } = [];
    }
}
