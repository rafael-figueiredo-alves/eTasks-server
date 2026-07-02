using eTasks_server.Models.Enums.Shopping;

namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    /// <summary>
    /// Representa a requisição de sincronização de dados de Compras
    /// </summary>
    public class ShoppingListPushSyncItemRequest
    {
        /// <summary>
        /// Identificador da opeação no cliente
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da operação
        /// </summary>
        public ShoppingListPushOperationType Operation { get; set; }

        /// <summary>
        /// Id da lista de compras
        /// </summary>
        public Guid? ShoppingListId { get; set; }

        /// <summary>
        /// Etag esperada
        /// </summary>
        public string? ExpectedEtag { get; set; }

        /// <summary>
        /// Requisição para criar lista de compras
        /// </summary>
        public CreateShoppingListRequest? Create { get; set; }

        /// <summary>
        /// Requisição para atualizar lista de compras
        /// </summary>
        public UpdateShoppingListRequest? Update { get; set; }
    }
}
