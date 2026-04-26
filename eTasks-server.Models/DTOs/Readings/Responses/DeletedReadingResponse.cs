namespace eTasks_server.Models.DTOs.Readings.Responses
{
    public class DeletedReadingResponse
    {
        public Guid Id { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
