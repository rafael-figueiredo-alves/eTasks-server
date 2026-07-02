namespace eTasks_server.Models.DTOs.ServerSettings.Requests
{
    /// <summary>
    /// Representa dados de configuração do servidor para atualização.
    /// </summary>
    public class UpdateServerSettingsRequest
    {
        /// <summary>
        /// Habilita ou desabilita o envio de e-mails via SMTP.
        /// </summary>
        public bool SmtpEnabled { get; set; }

        /// <summary>
        /// O host do servidor SMTP para envio de e-mails.
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// A porta do servidor SMTP para envio de e-mails.
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Indica se a conexão SMTP deve usar SSL para segurança.
        /// </summary>
        public bool SmtpEnableSsl { get; set; } = true;

        /// <summary>
        /// O nome de usuário para autenticação no servidor SMTP.
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// A senha para autenticação no servidor SMTP.
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// O endereço de e-mail do remetente para envio de e-mails via SMTP.
        /// </summary>
        public string SmtpFromEmail { get; set; } = string.Empty;

        /// <summary>
        /// O nome do remetente para envio de e-mails via SMTP.
        /// </summary>
        public string SmtpFromName { get; set; } = string.Empty;

        /// <summary>
        /// Indica se a integração com o OpenRouter está habilitada.
        /// </summary>

        public bool OpenRouterEnabled { get; set; }

        /// <summary>
        /// A URL base da API do OpenRouter para integração.
        /// </summary>
        public string OpenRouterBaseUrl { get; set; } = "https://openrouter.ai/api/v1/";

        /// <summary>
        /// A chave de API para autenticação com o OpenRouter.
        /// </summary>
        public string OpenRouterApiKey { get; set; } = string.Empty;

        /// <summary>
        /// O modelo do OpenRouter a ser utilizado para geração de respostas.
        /// </summary>
        public string OpenRouterModel { get; set; } = "meta-llama/llama-3.3-8b-instruct:free";

        /// <summary>
        /// A URL do site do OpenRouter para referência ou redirecionamento.
        /// </summary>
        public string OpenRouterSiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// O nome do aplicativo que será exibido ao interagir com o OpenRouter.
        /// </summary>
        public string OpenRouterAppName { get; set; } = "eTasks Server";

        /// <summary>
        /// A temperatura do modelo do OpenRouter, que influencia a criatividade das respostas geradas.
        /// </summary>
        public decimal OpenRouterTemperature { get; set; } = 0.3m;

        /// <summary>
        /// O número máximo de tokens que podem ser utilizados em uma única requisição ao OpenRouter.
        /// </summary>
        public int OpenRouterMaxTokens { get; set; } = 700;

        /// <summary>
        /// Indica se a auditoria de operações no MongoDB está habilitada.
        /// </summary>
        public bool MongoAuditEnabled { get; set; }

        /// <summary>
        /// A string de conexão para o banco de dados MongoDB onde os logs de auditoria serão armazenados.
        /// </summary>
        public string MongoAuditConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// O nome do banco de dados MongoDB onde os logs de auditoria serão armazenados.
        /// </summary>
        public string MongoAuditDatabaseName { get; set; } = "etasks_server";

        /// <summary>
        /// Nome da coleção dos dados
        /// </summary>
        public string MongoAuditCollectionName { get; set; } = "operation_audit_logs";

        /// <summary>
        /// Tempo de retenção do log
        /// </summary>
        public int ApplicationLogRetentionDays { get; set; } = 7;

        /// <summary>
        /// Prazo de validade para reativar conta via código enviado
        /// </summary>
        public int AccountReactivationCodeValidityDays { get; set; } = 30;

        /// <summary>
        /// Ativar ou desativar login via Google
        /// </summary>
        public bool GoogleOpenIdEnabled { get; set; }

        /// <summary>
        /// Identificador de ClientId do Google
        /// </summary>
        public string GoogleOpenIdClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret do Google
        /// </summary>
        public string GoogleOpenIdClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// URI de redirecionamento
        /// </summary>
        public string GoogleOpenIdRedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Redirecionamento em caso de sucesso no navegador
        /// </summary>
        public string GoogleOpenIdWebSuccessRedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// código state para validação da autenticação
        /// </summary>
        public string GoogleOpenIdStateCode { get; set; } = string.Empty;
    }
}
