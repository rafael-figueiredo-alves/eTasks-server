using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IApplicationLogAdminBLL
    {
        Task<IReadOnlyList<LogFileSummaryResponse>> GetFilesAsync(CancellationToken cancellationToken = default);
        Task<LogFileContentResponse> ReadFileAsync(string fileName, CancellationToken cancellationToken = default);
        Task<LogFileDownloadResponse> DownloadFileAsync(string fileName, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
