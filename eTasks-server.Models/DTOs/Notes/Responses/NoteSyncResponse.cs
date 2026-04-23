namespace eTasks_server.Models.DTOs.Notes.Responses
{
    public class NoteSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<NoteDetailsResponse> Upserts { get; set; } = [];
        public List<DeletedNoteResponse> Deleted { get; set; } = [];
    }
}
