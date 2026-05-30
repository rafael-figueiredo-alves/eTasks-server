using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface do serviço administrativo de gerenciamento de Logs
    /// </summary>
    public interface IApplicationLogAdminService
    {
        /// <summary>
        /// Obtém os arquivos de log disponíveis.
        /// </summary>
        /// <param name="cancellationToken">Cancela operação</param>
        /// <returns>Lista de Logs disponíveis</returns>
        Task<IReadOnlyList<LogFileSummaryResponse>> GetFilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ler arquivo de log e exibir seu conteúdo
        /// </summary>
        /// <param name="fileName">Nome do arquivo de log</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns>Conteúdo do arquivo de Log</returns>
        Task<LogFileContentResponse> ReadFileAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Apagar arquivo de log
        /// </summary>
        /// <param name="fileName">Nome do arquivo de log a apagar</param>
        /// <param name="cancellationToken">Cancelar operação</param>
        /// <returns></returns>
        Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    }
}
