namespace eTasks_server.Models.DTOs.Finances.Responses
{
    public class DeletedFinanceEntryResponse
    {
        public Guid Id { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
