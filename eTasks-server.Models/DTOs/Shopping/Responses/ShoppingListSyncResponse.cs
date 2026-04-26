namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    public class ShoppingListSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<ShoppingListDetailsResponse> Upserts { get; set; } = [];
        public List<DeletedShoppingListResponse> Deleted { get; set; } = [];
    }
}
