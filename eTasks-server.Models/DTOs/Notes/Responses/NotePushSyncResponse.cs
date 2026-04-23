namespace eTasks_server.Models.DTOs.Notes.Responses
{
    public class NotePushSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<NotePushSyncItemResponse> Results { get; set; } = [];
    }
}
