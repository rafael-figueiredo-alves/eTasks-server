using System.Text;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;
using eTasks_server.Models.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para consulta, leitura, download e exclusao de arquivos de log do painel administrativo.
    /// </summary>
    public class ApplicationLogAdminBLL(
        IOptions<ApplicationLogAdminOptions> options,
        IApplicationLogRetentionService retentionService,
        ILogger<IApplicationLogAdminBLL> logger) : IApplicationLogAdminBLL
    {
        private const int MaxReadBytes = 2 * 1024 * 1024;
        private readonly string _logsDirectoryPath = options.Value.LogsDirectoryPath;

        /// <summary>
        /// Retorna o resumo dos arquivos de log disponiveis.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista resumida de arquivos de log.</returns>
        public async Task<IReadOnlyList<LogFileSummaryResponse>> GetFilesAsync(CancellationToken cancellationToken = default)
        {
            await retentionService.ApplyRetentionAsync(cancellationToken);
            var directory = EnsureLogsDirectory();
            var files = Directory.EnumerateFiles(directory, "*.txt", SearchOption.TopDirectoryOnly)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTime)
                .Select(file => new LogFileSummaryResponse
                {
                    FileName = file.Name,
                    SizeBytes = file.Length,
                    CreatedAt = file.CreationTime,
                    LastModifiedAt = file.LastWriteTime
                })
                .ToList();

            return files;
        }

        /// <summary>
        /// Lera o conteudo de um arquivo de log.
        /// </summary>
        /// <param name="fileName">Nome do arquivo de log.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Conteudo textual do arquivo.</returns>
        public async Task<LogFileContentResponse> ReadFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var path = ResolveLogFilePath(fileName, mustExist: true);
            var fileInfo = new FileInfo(path);

            if (fileInfo.Length > MaxReadBytes)
            {
                throw new ValidationException(nameof(fileName), $"O arquivo tem {FormatBytes(fileInfo.Length)}. Baixe o arquivo para visualizar localmente.");
            }

            var content = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            return new LogFileContentResponse
            {
                FileName = fileInfo.Name,
                Content = content,
                SizeBytes = fileInfo.Length,
                LastModifiedAt = fileInfo.LastWriteTime
            };
        }

        /// <summary>
        /// Faz o download binario de um arquivo de log.
        /// </summary>
        /// <param name="fileName">Nome do arquivo de log.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Arquivo para download.</returns>
        public async Task<LogFileDownloadResponse> DownloadFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var path = ResolveLogFilePath(fileName, mustExist: true);
            var fileInfo = new FileInfo(path);

            return new LogFileDownloadResponse
            {
                FileName = fileInfo.Name,
                Content = await File.ReadAllBytesAsync(path, cancellationToken)
            };
        }

        /// <summary>
        /// Remove um arquivo de log.
        /// </summary>
        /// <param name="fileName">Nome do arquivo de log.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var path = ResolveLogFilePath(fileName, mustExist: true);
            File.Delete(path);
            logger.LogInformation("Arquivo de log {FileName} removido pelo painel administrativo.", Path.GetFileName(path));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Garante que o diretorio de logs exista e retorna seu caminho absoluto.
        /// </summary>
        /// <returns>Caminho absoluto do diretorio de logs.</returns>
        private string EnsureLogsDirectory()
        {
            if (string.IsNullOrWhiteSpace(_logsDirectoryPath))
            {
                throw new InvalidOperationException("Diretorio de logs nao configurado.");
            }

            Directory.CreateDirectory(_logsDirectoryPath);
            return Path.GetFullPath(_logsDirectoryPath);
        }

        /// <summary>
        /// Resolve e valida o caminho completo de um arquivo de log.
        /// </summary>
        /// <param name="fileName">Nome do arquivo de log.</param>
        /// <param name="mustExist">Indica se o arquivo deve existir.</param>
        /// <returns>Caminho absoluto do arquivo de log.</returns>
        private string ResolveLogFilePath(string fileName, bool mustExist)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
            {
                throw new ValidationException(nameof(fileName), "Nome de arquivo de log invalido.");
            }

            var directory = EnsureLogsDirectory();
            var path = Path.GetFullPath(Path.Combine(directory, fileName));

            if (!path.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(nameof(fileName), "Nome de arquivo de log invalido.");
            }

            if (mustExist && !File.Exists(path))
            {
                throw new FileNotFoundException("Arquivo de log nao encontrado.", fileName);
            }

            return path;
        }

        /// <summary>
        /// Formata a quantidade de bytes em uma unidade legivel.
        /// </summary>
        /// <param name="bytes">Quantidade de bytes.</param>
        /// <returns>Texto formatado com unidade.</returns>
        private static string FormatBytes(long bytes)
        {
            string[] units = ["B", "KB", "MB", "GB", "TB"];
            var value = (double)bytes;
            var unit = 0;

            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }

            return $"{value:0.##} {units[unit]}";
        }
    }
}
