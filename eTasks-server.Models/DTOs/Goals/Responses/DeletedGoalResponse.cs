namespace eTasks_server.Models.DTOs.Goals.Responses
{
    public class DeletedGoalResponse
    {
        public Guid Id { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
