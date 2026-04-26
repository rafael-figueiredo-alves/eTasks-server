namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    public class ShoppingListPushSyncItemResponse
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public ShoppingListPushSyncItemStatus Status { get; set; }
        public ShoppingListDetailsResponse? ShoppingList { get; set; }
        public DeletedShoppingListResponse? Deleted { get; set; }
        public string? ServerEtag { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
