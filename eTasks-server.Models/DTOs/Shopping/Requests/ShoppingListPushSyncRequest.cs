namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Lista com operações de sincronização de dados
    /// </summary>
    public class ShoppingListPushSyncRequest
    {
        /// <summary>
        /// Lista com operações de sincronização de dados
        /// </summary>
        public List<ShoppingListPushSyncItemRequest> Operations { get; set; } = [];
    }
}
