using eTasks_server.Models.DTOs.Shopping.Requests;
using eTasks_server.Models.DTOs.Shopping.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface do recurso de Compras
    /// </summary>
    public interface IShoppingListBLL
    {
        /// <summary>
        /// Obtem listas de Compras
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<ShoppingListListItemResponse>> ListAsync(Guid userUid, ListShoppingListsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem uma lista de compras pelo id informado
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ShoppingListDetailsResponse> GetByIdAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria uma nova lista
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ShoppingListDetailsResponse> CreateAsync(Guid userUid, CreateShoppingListRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza a lista de compras
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ShoppingListDetailsResponse> UpdateAsync(Guid userUid, Guid shoppingListId, UpdateShoppingListRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exclui a lista de compras
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="shoppingListId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza dados
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ShoppingListSyncResponse> SyncAsync(Guid userUid, SyncShoppingListsRequest request, CancellationToken cancellationToken = default);
    }
}
