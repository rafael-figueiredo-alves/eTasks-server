namespace eTasks_server.Models.DTOs.Notes.Requests
{
    public class NotePushSyncItemRequest
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public NotePushOperationType Operation { get; set; }
        public Guid? NoteId { get; set; }
        public string? ExpectedEtag { get; set; }
        public CreateNoteRequest? Create { get; set; }
        public UpdateNoteRequest? Update { get; set; }
    }
}
