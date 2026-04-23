namespace eTasks_server.Models.DTOs.Notes.Responses
{
    public class NotePushSyncItemResponse
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public NotePushSyncItemStatus Status { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public NoteDetailsResponse? Note { get; set; }
        public DeletedNoteResponse? Deleted { get; set; }
        public string? ServerEtag { get; set; }
    }
}
