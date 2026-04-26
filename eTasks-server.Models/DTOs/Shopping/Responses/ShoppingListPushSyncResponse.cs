namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    public class ShoppingListPushSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<ShoppingListPushSyncItemResponse> Results { get; set; } = [];
    }
}
