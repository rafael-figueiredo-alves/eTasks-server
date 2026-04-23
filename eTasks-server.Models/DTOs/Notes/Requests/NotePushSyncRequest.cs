namespace eTasks_server.Models.DTOs.Notes.Requests
{
    public class NotePushSyncRequest
    {
        public List<NotePushSyncItemRequest> Operations { get; set; } = [];
    }
}
