using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Client.Services
{
    public class ApplicationLogAdminService(IApplicationLogAdminBLL bll) : IApplicationLogAdminService
    {
        public Task<IReadOnlyList<LogFileSummaryResponse>> GetFilesAsync(CancellationToken cancellationToken = default)
            => bll.GetFilesAsync(cancellationToken);

        public Task<LogFileContentResponse> ReadFileAsync(string fileName, CancellationToken cancellationToken = default)
            => bll.ReadFileAsync(fileName, cancellationToken);

        public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
            => bll.DeleteFileAsync(fileName, cancellationToken);
    }
}
