namespace eTasks_server.Models.DTOs.Notes.Responses
{
    public class DeletedNoteResponse
    {
        public Guid Id { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
