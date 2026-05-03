namespace eTasks_server.Models.DTOs.ServerSettings.Responses
{
    public class ServerSettingsResponse
    {
        public bool SmtpEnabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SmtpFromEmail { get; set; } = string.Empty;
        public string SmtpFromName { get; set; } = string.Empty;

        public bool OpenRouterEnabled { get; set; }
        public string OpenRouterBaseUrl { get; set; } = string.Empty;
        public string OpenRouterApiKey { get; set; } = string.Empty;
        public string OpenRouterModel { get; set; } = string.Empty;
        public string OpenRouterSiteUrl { get; set; } = string.Empty;
        public string OpenRouterAppName { get; set; } = string.Empty;
        public decimal OpenRouterTemperature { get; set; }
        public int OpenRouterMaxTokens { get; set; }

        public bool MongoAuditEnabled { get; set; }
        public string MongoAuditConnectionString { get; set; } = string.Empty;
        public string MongoAuditDatabaseName { get; set; } = string.Empty;
        public string MongoAuditCollectionName { get; set; } = string.Empty;
        public int ApplicationLogRetentionDays { get; set; } = 7;

        public bool GoogleOpenIdEnabled { get; set; }
        public string GoogleOpenIdClientId { get; set; } = string.Empty;
        public string GoogleOpenIdClientSecret { get; set; } = string.Empty;
        public string GoogleOpenIdRedirectUri { get; set; } = string.Empty;
        public string GoogleOpenIdWebSuccessRedirectUrl { get; set; } = string.Empty;
        public string GoogleOpenIdStateCode { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}
