namespace eTasks_server.Models.DTOs.Shopping.Responses
{
    public class DeletedShoppingListResponse
    {
        public Guid Id { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
