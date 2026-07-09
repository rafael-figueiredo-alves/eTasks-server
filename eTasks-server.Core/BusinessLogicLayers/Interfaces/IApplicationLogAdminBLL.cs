using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Regras de negócio de manipulação dos arquivos de log
    /// </summary>
    public interface IApplicationLogAdminBLL
    {
        /// <summary>
        /// Obtem os arquivos de log
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IReadOnlyList<LogFileSummaryResponse>> GetFilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lê arquivo de log especificado
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<LogFileContentResponse> ReadFileAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Baixa arquivo de log
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<LogFileDownloadResponse> DownloadFileAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Apaga arquivo de log
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
