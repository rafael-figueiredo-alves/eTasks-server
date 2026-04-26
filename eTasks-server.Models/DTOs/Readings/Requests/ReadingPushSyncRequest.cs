namespace eTasks_server.Models.DTOs.Readings.Requests
{
    public class ReadingPushSyncRequest
    {
        public List<ReadingPushSyncItemRequest> Operations { get; set; } = [];
    }
}
