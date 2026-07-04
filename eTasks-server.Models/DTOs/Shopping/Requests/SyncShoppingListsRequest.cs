namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Classe de suporte da sincronização das listas de compras
    /// </summary>
    public class SyncShoppingListsRequest
    {
        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
