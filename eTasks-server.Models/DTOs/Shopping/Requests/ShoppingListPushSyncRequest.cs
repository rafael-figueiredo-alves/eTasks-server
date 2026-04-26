namespace eTasks_server.Models.DTOs.Shopping.Requests
{
    public class ShoppingListPushSyncRequest
    {
        public List<ShoppingListPushSyncItemRequest> Operations { get; set; } = [];
    }
}
