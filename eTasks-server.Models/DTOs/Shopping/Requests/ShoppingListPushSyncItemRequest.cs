namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    public class ShoppingListPushSyncItemRequest
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public ShoppingListPushOperationType Operation { get; set; }
        public Guid? ShoppingListId { get; set; }
        public string? ExpectedEtag { get; set; }
        public CreateShoppingListRequest? Create { get; set; }
        public UpdateShoppingListRequest? Update { get; set; }
    }
}
