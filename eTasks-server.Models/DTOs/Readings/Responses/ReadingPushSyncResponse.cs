namespace eTasks_server.Models.DTOs.Readings.Responses
{
    public class ReadingPushSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<ReadingPushSyncItemResponse> Results { get; set; } = [];
    }
}
