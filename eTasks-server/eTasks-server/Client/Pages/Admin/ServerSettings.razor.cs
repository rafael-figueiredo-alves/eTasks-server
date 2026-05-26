using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Helpers;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class ServerSettingsPage : ComponentBase
    {
        #region Serviços Injetados
        [Inject] private IServerSettingsAdminService ServerSettingsService { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        #endregion

        #region Variáveis
        protected UpdateServerSettingsRequest _model = new();
        protected bool _isLoading = true;
        protected bool _isBusy;
        protected string _statusMessage = string.Empty;
        protected Severity _statusSeverity = Severity.Info;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            await ReloadAsync();
        }

        protected async Task ReloadAsync()
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                _isLoading = true;
                var response = await ServerSettingsService.GetAsync();
                _model = MapRequest(response);
                SetStatus($"Configurações carregadas. Última atualização: {response.UpdatedAt:dd/MM/yyyy HH:mm}.", Severity.Info);
            }, "Erro ao carregar configurações do servidor.", Snackbar, value => _isBusy = value, SetStatus);

            _isLoading = false;
        }

        protected async Task SaveAsync()
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                var saved = await ServerSettingsService.UpdateAsync(_model);
                _model = MapRequest(saved);
                SetStatus($"Configurações salvas em {saved.UpdatedAt:dd/MM/yyyy HH:mm}.", Severity.Success);
            }, "Erro ao salvar configurações do servidor.", Snackbar, value => _isBusy = value, SetStatus);
        }

        protected Task TestEmailAsync()
            => ExecuteTestAsync(() => ServerSettingsService.TestEmailAsync(_model));

        protected Task TestOpenRouterAsync()
            => ExecuteTestAsync(() => ServerSettingsService.TestOpenRouterAsync(_model));

        protected Task TestMongoAsync()
            => ExecuteTestAsync(() => ServerSettingsService.TestMongoAsync(_model));

        private async Task ExecuteTestAsync(Func<Task<ServerSettingsTestResultResponse>> action)
        {
            await ThreadHelper.ExecuteBusyAsync(async () =>
            {
                var result = await action();
                SetStatus(result.Message, result.Success ? Severity.Success : Severity.Warning);
            }, "Falha ao executar o teste.", Snackbar, value => _isBusy = value, SetStatus);
        }

        private void SetStatus(string message, Severity severity)
        {
            _statusMessage = message;
            _statusSeverity = severity;
        }

        private static UpdateServerSettingsRequest MapRequest(ServerSettingsResponse response)
        {
            return new UpdateServerSettingsRequest
            {
                SmtpEnabled = response.SmtpEnabled,
                SmtpHost = response.SmtpHost,
                SmtpPort = response.SmtpPort,
                SmtpEnableSsl = response.SmtpEnableSsl,
                SmtpUsername = response.SmtpUsername,
                SmtpPassword = response.SmtpPassword,
                SmtpFromEmail = response.SmtpFromEmail,
                SmtpFromName = response.SmtpFromName,
                OpenRouterEnabled = response.OpenRouterEnabled,
                OpenRouterBaseUrl = response.OpenRouterBaseUrl,
                OpenRouterApiKey = response.OpenRouterApiKey,
                OpenRouterModel = response.OpenRouterModel,
                OpenRouterSiteUrl = response.OpenRouterSiteUrl,
                OpenRouterAppName = response.OpenRouterAppName,
                OpenRouterTemperature = response.OpenRouterTemperature,
                OpenRouterMaxTokens = response.OpenRouterMaxTokens,
                MongoAuditEnabled = response.MongoAuditEnabled,
                MongoAuditConnectionString = response.MongoAuditConnectionString,
                MongoAuditDatabaseName = response.MongoAuditDatabaseName,
                MongoAuditCollectionName = response.MongoAuditCollectionName,
                ApplicationLogRetentionDays = response.ApplicationLogRetentionDays,
                AccountReactivationCodeValidityDays = response.AccountReactivationCodeValidityDays,
                GoogleOpenIdEnabled = response.GoogleOpenIdEnabled,
                GoogleOpenIdClientId = response.GoogleOpenIdClientId,
                GoogleOpenIdClientSecret = response.GoogleOpenIdClientSecret,
                GoogleOpenIdRedirectUri = response.GoogleOpenIdRedirectUri,
                GoogleOpenIdWebSuccessRedirectUrl = response.GoogleOpenIdWebSuccessRedirectUrl,
                GoogleOpenIdStateCode = response.GoogleOpenIdStateCode
            };
        }
        #endregion
    }
}
