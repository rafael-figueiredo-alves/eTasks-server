using eTasks_server.Models.DTOs.Shopping.Requests;
using eTasks_server.Models.DTOs.Shopping.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IShoppingListBLL
    {
        Task<List<ShoppingListListItemResponse>> ListAsync(Guid userUid, ListShoppingListsRequest request, CancellationToken cancellationToken = default);
        Task<ShoppingListDetailsResponse> GetByIdAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default);
        Task<ShoppingListDetailsResponse> CreateAsync(Guid userUid, CreateShoppingListRequest request, CancellationToken cancellationToken = default);
        Task<ShoppingListDetailsResponse> UpdateAsync(Guid userUid, Guid shoppingListId, UpdateShoppingListRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default);
        Task<ShoppingListSyncResponse> SyncAsync(Guid userUid, SyncShoppingListsRequest request, CancellationToken cancellationToken = default);
    }
}
