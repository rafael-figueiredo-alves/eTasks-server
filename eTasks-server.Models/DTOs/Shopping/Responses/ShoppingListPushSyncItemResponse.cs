using eTasks_server.Models.Enums.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    /// <summary>
    /// Resposta da sincronização da lista de compras
    /// </summary>
    public class ShoppingListPushSyncItemResponse
    {
        /// <summary>
        /// Identificador no cliente
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Status da sincronização
        /// </summary>
        public ShoppingListPushSyncItemStatus Status { get; set; }

        /// <summary>
        /// Lista de compras
        /// </summary>
        public ShoppingListDetailsResponse? ShoppingList { get; set; }

        /// <summary>
        /// Listas excluídas
        /// </summary>
        public DeletedShoppingListResponse? Deleted { get; set; }

        /// <summary>
        /// Etag do servidor
        /// </summary>
        public string? ServerEtag { get; set; }

        /// <summary>
        /// Código de erro
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Mensagem de erro
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
