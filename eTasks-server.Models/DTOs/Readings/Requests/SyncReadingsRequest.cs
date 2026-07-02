namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// resultado da sincronização de leituras
    /// </summary>
    public class SyncReadingsRequest
    {
        /// <summary>
        /// Data de início para sincronização de leituras
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
