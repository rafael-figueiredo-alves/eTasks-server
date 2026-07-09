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
    /// <summary>
    /// Classe de negócio das configurações do servidor
    /// </summary>
    /// <param name="context">Contyexto do banco de dados</param>
    /// <param name="secretProtector">Classe protetora dos dados</param>
    /// <param name="diagnosticsService">Serviço diagnostico de APIs externas</param>
    /// <param name="logger">Serviço de log de erros do sistema</param>
    public class ServerSettingsAdminBLL(
        AppDbContext context,
        ISecretProtector secretProtector,
        IServerSettingsDiagnosticsService diagnosticsService,
        ILogger<IServerSettingsAdminBLL> logger) : BaseBLL<IServerSettingsAdminBLL>(context, logger), IServerSettingsAdminBLL
    {
        /// <summary>
        /// Função que retorna as configurações do servidor
        /// </summary>
        /// <param name="cancellationToken">Cancela operação</param>
        /// <returns>Resposta com configurações do servidor</returns>
        public async Task<ServerSettingsResponse> GetAsync(CancellationToken cancellationToken = default)
        {
            // Pega os dados ou pega valores padrões da classe
            var entity = await _context.ServerSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == 1, cancellationToken)
                ?? new ServerSettingsEntity();

            // Retorna entidade mapeada
            return MapResponse(entity);
        }

        /// <summary>
        /// Função que atualiza as configurações do servidor
        /// </summary>
        /// <param name="request">Recebe os valores a atualizar</param>
        /// <param name="cancellationToken">Token para cancelar operação</param>
        /// <returns>Configurações atualizadas</returns>
        public async Task<ServerSettingsResponse> UpdateAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida os dados enviados
            ValidateRequest(request);

            // Obtém os dados
            var entity = await _context.ServerSettings.FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);

            // Valida se haviam dados salvos e se não houverem indica que é novo registro
            var isNew = entity is null;

            // se entidave vazia, gera valores padrões e salva no banco
            entity ??= new ServerSettingsEntity();

            // Método que aplica as alterações
            Apply(entity, request);

            // seta data de atualização
            entity.UpdatedAt = SaoPauloDateTime.Now();

            // se for novo, seta a data de criação e adiciona entidade no pipe do entity framework
            if (isNew)
            {
                entity.CreatedAt = entity.UpdatedAt;
                await _context.ServerSettings.AddAsync(entity, cancellationToken);
            }

            // Salva entidade 
            await SaveChangesContextAsync(cancellationToken);

            // Mapeia resposta com configurações do servidor
            return MapResponse(entity);
        }

        /// <summary>
        /// Método que realiza o teste de envio de e-mail com SMTP
        /// </summary>
        /// <param name="request">Dados das configurações</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Configurações</returns>
        public Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida dados SMTP
            ValidateSmtp(request);

            // Chama a função do serviço de diagnostico para testar as configurações de e-mail
            return diagnosticsService.TestEmailAsync(request, cancellationToken);
        }

        /// <summary>
        /// Realiza teste das configurações do OpenRouter
        /// </summary>
        /// <param name="request">Dados das configurações de serviço</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados das configurações</returns>
        public Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida dados do OpenRouter
            ValidateOpenRouter(request);

            // realiza teste e retorna resultado
            return diagnosticsService.TestOpenRouterAsync(request, cancellationToken);
        }

        /// <summary>
        /// Realiza teste da conexão com o serviço do MongoDB
        /// </summary>
        /// <param name="request">Configurações do servidor</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta dos dados das configurações do servidor</returns>
        public Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida dados de conexão do MongoDb
            ValidateMongo(request);

            // Testa configurações
            return diagnosticsService.TestMongoAsync(request, cancellationToken);
        }

        #region Métodos privados específicos da regra de negócio
        /// <summary>
        /// Valida requisição
        /// </summary>
        /// <param name="request"></param>
        private void ValidateRequest(UpdateServerSettingsRequest request)
        {
            // Valida configurações de SMTP
            ValidateSmtp(request);

            // Valida configurações de OpenRouter (IA)
            ValidateOpenRouter(request);

            // Valida configurações do Mongo
            ValidateMongo(request);

            // Valida configurações da retenção de logs
            ValidateApplicationLogs(request);

            // Valida configurações da restauração de contas
            ValidateAccountRecovery(request);

            // Valida configurações do Login com Google
            ValidateGoogleOpenId(request);
        }

        /// <summary>
        /// Valida configurações de SMTP
        /// </summary>
        /// <param name="request">Dados de configurações</param>
        /// <exception cref="ValidationException">Exceção com erro de validação</exception>
        private static void ValidateSmtp(UpdateServerSettingsRequest request)
        {
            // Verifica se SMTP está habilitado
            if (!request.SmtpEnabled)
            {
                return;
            }

            // Verifica se Host está vazio
            if (string.IsNullOrWhiteSpace(request.SmtpHost))
            {
                throw new ValidationException(nameof(request.SmtpHost), "Informe o host SMTP.");
            }

            // Verifica se porta está configurada com valor entre 1 e 65535
            if (request.SmtpPort is < 1 or > 65535)
            {
                throw new ValidationException(nameof(request.SmtpPort), "Informe uma porta SMTP valida.");
            }

            // Valida se endereço de e-mail de retorno está vazio
            if (string.IsNullOrWhiteSpace(request.SmtpFromEmail))
            {
                throw new ValidationException(nameof(request.SmtpFromEmail), "Informe o e-mail remetente.");
            }
        }

        /// <summary>
        /// Valida configurações de OpenRouter
        /// </summary>
        /// <param name="request">Dados de configurações</param>
        /// <exception cref="ValidationException">Exceção com erro de validação</exception>
        private static void ValidateOpenRouter(UpdateServerSettingsRequest request)
        {
            // Valida se está habilitado
            if (!request.OpenRouterEnabled)
            {
                return;
            }

            // Valida se API KEy está vazia
            if (string.IsNullOrWhiteSpace(request.OpenRouterApiKey))
            {
                throw new ValidationException(nameof(request.OpenRouterApiKey), "Informe a API key do OpenRouter.");
            }

            // Valida se modelo está vazio
            if (string.IsNullOrWhiteSpace(request.OpenRouterModel))
            {
                throw new ValidationException(nameof(request.OpenRouterModel), "Informe o modelo do OpenRouter.");
            }

            // Valida se o modelo é um dos grátis disponíveis
            if (!request.OpenRouterModel.Contains(":free", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(nameof(request.OpenRouterModel), "Use um modelo gratuito do OpenRouter, finalizado com ':free'.");
            }

            // Valida se a temperatura está configurada fora do intervalo de 0 e 2
            if (request.OpenRouterTemperature is < 0m or > 2m)
            {
                throw new ValidationException(nameof(request.OpenRouterTemperature), "A temperatura deve ficar entre 0 e 2.");
            }

            // Valida se a quantidade máxima de tokens está fora dos limites entre 1 e 32000
            if (request.OpenRouterMaxTokens is < 1 or > 32000)
            {
                throw new ValidationException(nameof(request.OpenRouterMaxTokens), "Informe um limite de tokens valido.");
            }
        }

        /// <summary>
        /// Valida configurações de MongoDB
        /// </summary>
        /// <param name="request">Dados de configurações</param>
        /// <exception cref="ValidationException">Exceção com erro de validação</exception>
        private static void ValidateMongo(UpdateServerSettingsRequest request)
        {
            // Valida se está habilitado
            if (!request.MongoAuditEnabled)
            {
                return;
            }

            // Valida se a string de conexão está vazia
            if (string.IsNullOrWhiteSpace(request.MongoAuditConnectionString))
            {
                throw new ValidationException(nameof(request.MongoAuditConnectionString), "Informe a connection string do MongoDB.");
            }

            // Valida se o campo database está vazio
            if (string.IsNullOrWhiteSpace(request.MongoAuditDatabaseName))
            {
                throw new ValidationException(nameof(request.MongoAuditDatabaseName), "Informe o nome do banco MongoDB.");
            }

            // Valida se o campo coleção está vazio
            if (string.IsNullOrWhiteSpace(request.MongoAuditCollectionName))
            {
                throw new ValidationException(nameof(request.MongoAuditCollectionName), "Informe o nome da colecao MongoDB.");
            }
        }

        /// <summary>
        /// Valida configurações de Retenção de logs
        /// </summary>
        /// <param name="request">Dados de configurações</param>
        /// <exception cref="ValidationException">Exceção com erro de validação</exception>
        private static void ValidateApplicationLogs(UpdateServerSettingsRequest request)
        {
            // Valida se a retençao de logs está dentro de 2 e 15 dias
            if (request.ApplicationLogRetentionDays is < 2 or > 15)
            {
                throw new ValidationException(nameof(request.ApplicationLogRetentionDays), "A retenção dos logs deve ficar entre 2 e 15 dias.");
            }
        }

        /// <summary>
        /// Valida configurações de recuperação de conta
        /// </summary>
        /// <param name="request">Dados de configurações</param>
        /// <exception cref="ValidationException">Exceção com erro de validação</exception>
        private static void ValidateAccountRecovery(UpdateServerSettingsRequest request)
        {
            // Valida se o intervalo de reativação de conta está dentro de 7 a 90 dias
            if (request.AccountReactivationCodeValidityDays is < 7 or > 90)
            {
                throw new ValidationException(nameof(request.AccountReactivationCodeValidityDays), "A validade do link de reativação deve ficar entre 7 e 90 dias.");
            }
        }

        /// <summary>
        /// Valida configurações de Login com Google
        /// </summary>
        /// <param name="request">Dados de configurações</param>
        /// <exception cref="ValidationException">Exceção com erro de validação</exception>
        private static void ValidateGoogleOpenId(UpdateServerSettingsRequest request)
        {
            // Valida se habilitado o serviço
            if (!request.GoogleOpenIdEnabled)
            {
                return;
            }

            // Valida se Client ID está preenchido
            if (string.IsNullOrWhiteSpace(request.GoogleOpenIdClientId))
            {
                throw new ValidationException(nameof(request.GoogleOpenIdClientId), "Informe o Client ID do Google.");
            }

            // Valida se client secret está preenchido
            if (string.IsNullOrWhiteSpace(request.GoogleOpenIdClientSecret))
            {
                throw new ValidationException(nameof(request.GoogleOpenIdClientSecret), "Informe o Client Secret do Google.");
            }

            // Valida se a URL de redirecionamento está preenchida
            if (!string.IsNullOrWhiteSpace(request.GoogleOpenIdRedirectUri)
                && !Uri.TryCreate(request.GoogleOpenIdRedirectUri, UriKind.Absolute, out _))
            {
                throw new ValidationException(nameof(request.GoogleOpenIdRedirectUri), "Informe uma Redirect URI absoluta.");
            }

            // Valida se a URL de redirecionamento web está preenchida
            if (!string.IsNullOrWhiteSpace(request.GoogleOpenIdWebSuccessRedirectUrl)
                && !Uri.TryCreate(request.GoogleOpenIdWebSuccessRedirectUrl, UriKind.Absolute, out _))
            {
                throw new ValidationException(nameof(request.GoogleOpenIdWebSuccessRedirectUrl), "Informe uma URL absoluta para retorno web/PWA.");
            }

            // Valida se o State Code está preenchido e se o tamanho do mesmo é de no mínimo 16 digitos
            if (string.IsNullOrWhiteSpace(request.GoogleOpenIdStateCode) || request.GoogleOpenIdStateCode.Trim().Length < 16)
            {
                throw new ValidationException(nameof(request.GoogleOpenIdStateCode), "Informe um código fixo de state com pelo menos 16 caracteres.");
            }
        }

        /// <summary>
        /// Preenche a classe das configurações de servidor a ser persistida
        /// </summary>
        /// <param name="entity">Entidade a ser persistida</param>
        /// <param name="request">Dados a persistir</param>
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
            entity.ApplicationLogRetentionDays = request.ApplicationLogRetentionDays;
            entity.AccountReactivationCodeValidityDays = request.AccountReactivationCodeValidityDays;

            entity.GoogleOpenIdEnabled = request.GoogleOpenIdEnabled;
            entity.GoogleOpenIdClientId = request.GoogleOpenIdClientId.Trim();
            entity.GoogleOpenIdClientSecret = secretProtector.Protect(request.GoogleOpenIdClientSecret.Trim());
            entity.GoogleOpenIdRedirectUri = request.GoogleOpenIdRedirectUri.Trim();
            entity.GoogleOpenIdWebSuccessRedirectUrl = request.GoogleOpenIdWebSuccessRedirectUrl.Trim();
            entity.GoogleOpenIdStateCode = request.GoogleOpenIdStateCode.Trim();
        }

        /// <summary>
        /// Mapear resposta
        /// </summary>
        /// <param name="entity">entidade com configurações do servidor a mapear</param>
        /// <returns>Dados de configurações de servidor</returns>
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
                ApplicationLogRetentionDays = entity.ApplicationLogRetentionDays is < 2 or > 15 ? 7 : entity.ApplicationLogRetentionDays,
                AccountReactivationCodeValidityDays = entity.AccountReactivationCodeValidityDays is < 7 or > 90 ? 30 : entity.AccountReactivationCodeValidityDays,
                GoogleOpenIdEnabled = entity.GoogleOpenIdEnabled,
                GoogleOpenIdClientId = entity.GoogleOpenIdClientId,
                GoogleOpenIdClientSecret = secretProtector.Unprotect(entity.GoogleOpenIdClientSecret),
                GoogleOpenIdRedirectUri = entity.GoogleOpenIdRedirectUri,
                GoogleOpenIdWebSuccessRedirectUrl = entity.GoogleOpenIdWebSuccessRedirectUrl,
                GoogleOpenIdStateCode = entity.GoogleOpenIdStateCode,
                UpdatedAt = entity.UpdatedAt
            };
        }
        #endregion
    }
}
