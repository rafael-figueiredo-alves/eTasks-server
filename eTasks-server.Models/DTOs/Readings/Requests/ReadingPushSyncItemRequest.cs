namespace eTasks_server.Models.DTOs.Readings.Requests
{
    public class ReadingPushSyncItemRequest
    {
        public string ClientMutationId { get; set; } = string.Empty;
        public ReadingPushOperationType Operation { get; set; }
        public Guid? ReadingId { get; set; }
        public string? ExpectedEtag { get; set; }
        public CreateReadingRequest? Create { get; set; }
        public UpdateReadingRequest? Update { get; set; }
        public UpdateReadingProgressRequest? Progress { get; set; }
    }
}
