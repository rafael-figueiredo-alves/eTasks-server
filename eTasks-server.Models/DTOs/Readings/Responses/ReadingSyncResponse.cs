namespace eTasks_server.Models.DTOs.Readings.Responses
{
    public class ReadingSyncResponse
    {
        public DateTime ServerTime { get; set; }
        public List<ReadingDetailsResponse> Upserts { get; set; } = [];
        public List<DeletedReadingResponse> Deleted { get; set; } = [];
    }
}
