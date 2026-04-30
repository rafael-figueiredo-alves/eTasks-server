using eTasks_server.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eTasks_server.Core.Services
{
    public class ApplicationLogRetentionService(
        IOptions<ApplicationLogAdminOptions> options,
        IServerSettingsProvider settingsProvider,
        ILogger<IApplicationLogRetentionService> logger) : IApplicationLogRetentionService
    {
        private const int DefaultRetentionDays = 7;
        private const int MinRetentionDays = 2;
        private const int MaxRetentionDays = 15;

        public async Task<int> ApplyRetentionAsync(CancellationToken cancellationToken = default)
        {
            var directory = options.Value.LogsDirectoryPath;
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return 0;
            }

            var settings = await settingsProvider.GetCurrentAsync(cancellationToken);
            var retentionDays = settings.ApplicationLogRetentionDays is >= MinRetentionDays and <= MaxRetentionDays
                ? settings.ApplicationLogRetentionDays
                : DefaultRetentionDays;
            var cutoff = DateTime.Now.AddDays(-retentionDays);
            var deleted = 0;

            foreach (var path in Directory.EnumerateFiles(directory, "log-*.txt", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var file = new FileInfo(path);
                if (file.LastWriteTime >= cutoff)
                {
                    continue;
                }

                try
                {
                    file.Delete();
                    deleted++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Falha ao remover arquivo de log expirado {FileName}.", file.Name);
                }
            }

            return deleted;
        }
    }
}
