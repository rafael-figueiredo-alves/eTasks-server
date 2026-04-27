using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerSettingsEntity = eTasks_server.Models.Entities.Settings.ServerSettings;

namespace eTasks_server.Core.BusinessLogicLayers.Admin.ServerSettings
{
    public class ServerSettingsAdminBLL(
        AppDbContext context,
        ISecretProtector secretProtector,
        IServerSettingsDiagnosticsService diagnosticsService,
        ILogger<IServerSettingsAdminBLL> logger) : BaseBLL<IServerSettingsAdminBLL>(context, logger), IServerSettingsAdminBLL
    {
        public async Task<ServerSettingsResponse> GetAsync(CancellationToken cancellationToken = default)
        {
            var entity = await _context.ServerSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken)
                ?? new ServerSettingsEntity();

            return MapResponse(entity);
        }

        public async Task<ServerSettingsResponse> UpdateAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);

            var entity = await _context.ServerSettings.FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);
            var isNew = entity is null;
            entity ??= new ServerSettingsEntity();

            Apply(entity, request);
            entity.UpdatedAt = SaoPauloDateTime.Now();

            if (isNew)
            {
                entity.CreatedAt = entity.UpdatedAt;
                await _context.ServerSettings.AddAsync(entity, cancellationToken);
            }

            await SaveChangesContextAsync(cancellationToken);
            return MapResponse(entity);
        }

        public Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            ValidateSmtp(request);
            return diagnosticsService.TestEmailAsync(request, cancellationToken);
        }

        public Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            ValidateOpenRouter(request);
            return diagnosticsService.TestOpenRouterAsync(request, cancellationToken);
        }

        public Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            ValidateMongo(request);
            return diagnosticsService.TestMongoAsync(request, cancellationToken);
        }

        private void ValidateRequest(UpdateServerSettingsRequest request)
        {
            ValidateSmtp(request);
            ValidateOpenRouter(request);
            ValidateMongo(request);
        }

        private static void ValidateSmtp(UpdateServerSettingsRequest request)
        {
            if (!request.SmtpEnabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.SmtpHost))
            {
                throw new ValidationException(nameof(request.SmtpHost), "Informe o host SMTP.");
            }

            if (request.SmtpPort is < 1 or > 65535)
            {
                throw new ValidationException(nameof(request.SmtpPort), "Informe uma porta SMTP valida.");
            }

            if (string.IsNullOrWhiteSpace(request.SmtpFromEmail))
            {
                throw new ValidationException(nameof(request.SmtpFromEmail), "Informe o e-mail remetente.");
            }
        }

        private static void ValidateOpenRouter(UpdateServerSettingsRequest request)
        {
            if (!request.OpenRouterEnabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.OpenRouterApiKey))
            {
                throw new ValidationException(nameof(request.OpenRouterApiKey), "Informe a API key do OpenRouter.");
            }

            if (string.IsNullOrWhiteSpace(request.OpenRouterModel))
            {
                throw new ValidationException(nameof(request.OpenRouterModel), "Informe o modelo do OpenRouter.");
            }

            if (!request.OpenRouterModel.Contains(":free", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(nameof(request.OpenRouterModel), "Use um modelo gratuito do OpenRouter, finalizado com ':free'.");
            }

            if (request.OpenRouterTemperature is < 0m or > 2m)
            {
                throw new ValidationException(nameof(request.OpenRouterTemperature), "A temperatura deve ficar entre 0 e 2.");
            }

            if (request.OpenRouterMaxTokens is < 1 or > 32000)
            {
                throw new ValidationException(nameof(request.OpenRouterMaxTokens), "Informe um limite de tokens valido.");
            }
        }

        private static void ValidateMongo(UpdateServerSettingsRequest request)
        {
            if (!request.MongoAuditEnabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.MongoAuditConnectionString))
            {
                throw new ValidationException(nameof(request.MongoAuditConnectionString), "Informe a connection string do MongoDB.");
            }

            if (string.IsNullOrWhiteSpace(request.MongoAuditDatabaseName))
            {
                throw new ValidationException(nameof(request.MongoAuditDatabaseName), "Informe o nome do banco MongoDB.");
            }

            if (string.IsNullOrWhiteSpace(request.MongoAuditCollectionName))
            {
                throw new ValidationException(nameof(request.MongoAuditCollectionName), "Informe o nome da colecao MongoDB.");
            }
        }

        private void Apply(ServerSettingsEntity entity, UpdateServerSettingsRequest request)
        {
            entity.SmtpEnabled = request.SmtpEnabled;
            entity.SmtpHost = request.SmtpHost.Trim();
            entity.SmtpPort = request.SmtpPort;
            entity.SmtpEnableSsl = request.SmtpEnableSsl;
            entity.SmtpUsername = request.SmtpUsername.Trim();
            entity.SmtpPassword = secretProtector.Protect(request.SmtpPassword.Trim());
            entity.SmtpFromEmail = request.SmtpFromEmail.Trim();
            entity.SmtpFromName = request.SmtpFromName.Trim();

            entity.OpenRouterEnabled = request.OpenRouterEnabled;
            entity.OpenRouterBaseUrl = string.IsNullOrWhiteSpace(request.OpenRouterBaseUrl)
                ? "https://openrouter.ai/api/v1/"
                : request.OpenRouterBaseUrl.Trim();
            entity.OpenRouterApiKey = secretProtector.Protect(request.OpenRouterApiKey.Trim());
            entity.OpenRouterModel = request.OpenRouterModel.Trim();
            entity.OpenRouterSiteUrl = request.OpenRouterSiteUrl.Trim();
            entity.OpenRouterAppName = request.OpenRouterAppName.Trim();
            entity.OpenRouterTemperature = request.OpenRouterTemperature;
            entity.OpenRouterMaxTokens = request.OpenRouterMaxTokens;

            entity.MongoAuditEnabled = request.MongoAuditEnabled;
            entity.MongoAuditConnectionString = secretProtector.Protect(request.MongoAuditConnectionString.Trim());
            entity.MongoAuditDatabaseName = request.MongoAuditDatabaseName.Trim();
            entity.MongoAuditCollectionName = request.MongoAuditCollectionName.Trim();
        }

        private ServerSettingsResponse MapResponse(ServerSettingsEntity entity)
        {
            return new ServerSettingsResponse
            {
                SmtpEnabled = entity.SmtpEnabled,
                SmtpHost = entity.SmtpHost,
                SmtpPort = entity.SmtpPort,
                SmtpEnableSsl = entity.SmtpEnableSsl,
                SmtpUsername = entity.SmtpUsername,
                SmtpPassword = secretProtector.Unprotect(entity.SmtpPassword),
                SmtpFromEmail = entity.SmtpFromEmail,
                SmtpFromName = entity.SmtpFromName,
                OpenRouterEnabled = entity.OpenRouterEnabled,
                OpenRouterBaseUrl = entity.OpenRouterBaseUrl,
                OpenRouterApiKey = secretProtector.Unprotect(entity.OpenRouterApiKey),
                OpenRouterModel = entity.OpenRouterModel,
                OpenRouterSiteUrl = entity.OpenRouterSiteUrl,
                OpenRouterAppName = entity.OpenRouterAppName,
                OpenRouterTemperature = entity.OpenRouterTemperature,
                OpenRouterMaxTokens = entity.OpenRouterMaxTokens,
                MongoAuditEnabled = entity.MongoAuditEnabled,
                MongoAuditConnectionString = secretProtector.Unprotect(entity.MongoAuditConnectionString),
                MongoAuditDatabaseName = entity.MongoAuditDatabaseName,
                MongoAuditCollectionName = entity.MongoAuditCollectionName,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
