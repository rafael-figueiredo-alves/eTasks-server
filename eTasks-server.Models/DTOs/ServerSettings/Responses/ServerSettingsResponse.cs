namespace eTasks_server.Models.DTOs.ServerSettings.Responses
{
    /// <summary>
    /// Resposta das configurações do servidor
    /// </summary>
    public class ServerSettingsResponse
    {
        /// <summary>
        /// Indica se Serviço de envio de emails está avito
        /// </summary>
        public bool SmtpEnabled { get; set; }

        /// <summary>
        /// Host do serviço de envio de emails
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// Porta do serviço de e-mail
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Serviço de envio de e-mail usa ou não SSL
        /// </summary>
        public bool SmtpEnableSsl { get; set; }

        /// <summary>
        /// Nome de usuário do serviço de e-mail
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// Senha da conta de serviço do email
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail a usar oa enviar e-mails
        /// </summary>
        public string SmtpFromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Nome do remetente dos e-mails
        /// </summary>
        public string SmtpFromName { get; set; } = string.Empty;

        /// <summary>
        /// Habilita / desabilita OpenRouter
        /// </summary>
        public bool OpenRouterEnabled { get; set; }

        /// <summary>
        /// URL base do OpenRouter
        /// </summary>
        public string OpenRouterBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Chave de API do Openrouter
        /// </summary>
        public string OpenRouterApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Modelo a usar do openrouter
        /// </summary>
        public string OpenRouterModel { get; set; } = string.Empty;

        /// <summary>
        /// URL do site a redirecionar do Openrouter
        /// </summary>
        public string OpenRouterSiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Nome do aplicativo OpenRouter
        /// </summary>
        public string OpenRouterAppName { get; set; } = string.Empty;

        /// <summary>
        /// Temperatura de repsosta do agente da Openrouter
        /// </summary>
        public decimal OpenRouterTemperature { get; set; }

        /// <summary>
        /// Qtd. Máxima de tokens
        /// </summary>
        public int OpenRouterMaxTokens { get; set; }


        /// <summary>
        /// Opção que habilita ou desabilita log gravado no MongoDB
        /// </summary>
        public bool MongoAuditEnabled { get; set; }

        /// <summary>
        /// String de conexão do banco mongoDB
        /// </summary>
        public string MongoAuditConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Nome do banco de dados MongoDB
        /// </summary>
        public string MongoAuditDatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Nome da coleção do MongoDB
        /// </summary>
        public string MongoAuditCollectionName { get; set; } = string.Empty;

        /// <summary>
        /// Tempo máximo de retenção de log
        /// </summary>
        public int ApplicationLogRetentionDays { get; set; } = 7;

        /// <summary>
        /// Validade máxima do código para reativar conta excluída
        /// </summary>
        public int AccountReactivationCodeValidityDays { get; set; } = 30;


        /// <summary>
        /// Habilitar / Desabilitar login via Google
        /// </summary>
        public bool GoogleOpenIdEnabled { get; set; }

        /// <summary>
        /// Client ID do Google
        /// </summary>
        public string GoogleOpenIdClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret do Google
        /// </summary>
        public string GoogleOpenIdClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// URL de redirecionamento do Google
        /// </summary>
        public string GoogleOpenIdRedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// URL de redirecionamento do Google na web
        /// </summary>
        public string GoogleOpenIdWebSuccessRedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// Código state
        /// </summary>
        public string GoogleOpenIdStateCode { get; set; } = string.Empty;

        /// <summary>
        /// Data/Hora da última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
