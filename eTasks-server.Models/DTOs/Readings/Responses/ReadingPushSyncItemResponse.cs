namespace eTasks_server.Models.DTOs.Readings.Responses
{
    public class ReadingPushSyncItemResponse
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public ReadingPushSyncItemStatus Status { get; set; }
        public ReadingDetailsResponse? Reading { get; set; }
        public DeletedReadingResponse? Deleted { get; set; }
        public string? ServerEtag { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
