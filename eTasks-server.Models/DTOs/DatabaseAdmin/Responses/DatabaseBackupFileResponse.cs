namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    public class DatabaseBackupFileResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/sql";
        public byte[] Content { get; set; } = [];
    }
}
