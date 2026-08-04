using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Classe responsável por fornecer as configurações do servidor, incluindo a descriptografia de senhas e chaves sensíveis.
    /// </summary>
    /// <param name="context">O contexto do banco de dados.</param>
    /// <param name="secretProtector">O serviço responsável por proteger e desproteger segredos.</param>
    public class ServerSettingsProvider(AppDbContext context, ISecretProtector secretProtector) : IServerSettingsProvider
    {
        /// <summary>
        /// Obtém as configurações atuais do servidor, descriptografando senhas e chaves sensíveis antes de retorná-las.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ServerSettings> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            // Obtém as configurações do servidor do banco de dados, sem rastreamento de alterações.
            var settings = await context.ServerSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken)
                ?? new ServerSettings();

            // Descriptografa as senhas e chaves sensíveis usando o serviço de proteção de segredos.
            settings.SmtpPassword = secretProtector.Unprotect(settings.SmtpPassword);
            settings.OpenRouterApiKey = secretProtector.Unprotect(settings.OpenRouterApiKey);
            settings.MongoAuditConnectionString = secretProtector.Unprotect(settings.MongoAuditConnectionString);
            settings.GoogleOpenIdClientSecret = secretProtector.Unprotect(settings.GoogleOpenIdClientSecret);

            return settings;
        }
    }
}
