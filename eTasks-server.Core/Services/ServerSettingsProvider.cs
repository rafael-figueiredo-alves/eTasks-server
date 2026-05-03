using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.Services
{
    public class ServerSettingsProvider(AppDbContext context, ISecretProtector secretProtector) : IServerSettingsProvider
    {
        public async Task<ServerSettings> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            var settings = await context.ServerSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken)
                ?? new ServerSettings();

            settings.SmtpPassword = secretProtector.Unprotect(settings.SmtpPassword);
            settings.OpenRouterApiKey = secretProtector.Unprotect(settings.OpenRouterApiKey);
            settings.MongoAuditConnectionString = secretProtector.Unprotect(settings.MongoAuditConnectionString);
            settings.GoogleOpenIdClientSecret = secretProtector.Unprotect(settings.GoogleOpenIdClientSecret);

            return settings;
        }
    }
}
