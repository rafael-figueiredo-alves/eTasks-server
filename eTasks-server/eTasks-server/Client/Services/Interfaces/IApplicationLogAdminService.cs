using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IApplicationLogAdminService
    {
        Task<IReadOnlyList<LogFileSummaryResponse>> GetFilesAsync(CancellationToken cancellationToken = default);
        Task<LogFileContentResponse> ReadFileAsync(string fileName, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
