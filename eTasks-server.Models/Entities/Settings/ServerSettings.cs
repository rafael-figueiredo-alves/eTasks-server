using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Settings
{
    /// <summary>
    /// Configurações do servidor
    /// </summary>
    public class ServerSettings : IEntityModelConfiguration<ServerSettings>
    {
        /// <summary>
        /// identificação
        /// </summary>
        public int Id { get; set; } = 1;

        /// <summary>
        /// Habilita SMTP
        /// </summary>
        public bool SmtpEnabled { get; set; }

        /// <summary>
        /// servidor para envio de e-mails
        /// </summary>
        public string SmtpHost { get; set; } = string.Empty;

        /// <summary>
        /// porta do servidor smtp
        /// </summary>
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// Habilitar SSL 
        /// </summary>
        public bool SmtpEnableSsl { get; set; } = true;

        /// <summary>
        /// Username do SMTP
        /// </summary>
        public string SmtpUsername { get; set; } = string.Empty;

        /// <summary>
        /// Senha do SMTP
        /// </summary>
        public string SmtpPassword { get; set; } = string.Empty;

        /// <summary>
        /// Endereço do e-mail de quem envia
        /// </summary>
        public string SmtpFromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Nome do remetente
        /// </summary>
        public string SmtpFromName { get; set; } = string.Empty;

        /// <summary>
        /// Habilitar OpenRouter
        /// </summary>
        public bool OpenRouterEnabled { get; set; }

        /// <summary>
        /// Url base do OpenRouter
        /// </summary>
        public string OpenRouterBaseUrl { get; set; } = "https://openrouter.ai/api/v1/";

        /// <summary>
        /// Habilitar chave de api do open router
        /// </summary>
        public string OpenRouterApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Modelo a usar com OpenRouter
        /// </summary>
        public string OpenRouterModel { get; set; } = "meta-llama/llama-3.3-8b-instruct:free";

        /// <summary>
        /// URL do site de redirecionamento do OpenRouter
        /// </summary>
        public string OpenRouterSiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Nome da aplicação no OpenRouter
        /// </summary>
        public string OpenRouterAppName { get; set; } = "eTasks Server";

        /// <summary>
        /// Temperatura da IA
        /// </summary>
        public decimal OpenRouterTemperature { get; set; } = 0.3m;

        /// <summary>
        /// Limite máximo de tokens
        /// </summary>
        public int OpenRouterMaxTokens { get; set; } = 700;

        /// <summary>
        /// Habilita auditoria/log Mongodb
        /// </summary>
        public bool MongoAuditEnabled { get; set; }
        
        /// <summary>
        /// String de conexão do Mongodb
        /// </summary>
        public string MongoAuditConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Nome da base de dados do Mongodb
        /// </summary>
        public string MongoAuditDatabaseName { get; set; } = "etasks_server";

        /// <summary>
        /// Nome do Banco de dados Mongodb (coleção)
        /// </summary>
        public string MongoAuditCollectionName { get; set; } = "operation_audit_logs";

        /// <summary>
        /// Limite em dias da retenção de logs
        /// </summary>
        public int ApplicationLogRetentionDays { get; set; } = 7;

        /// <summary>
        /// Validade em dias do código de reativação de conta
        /// </summary>
        public int AccountReactivationCodeValidityDays { get; set; } = 30;


        /// <summary>
        /// Habilitar autenticação OAUTH
        /// </summary>
        public bool GoogleOpenIdEnabled { get; set; }

        /// <summary>
        /// ClientID do Google
        /// </summary>
        public string GoogleOpenIdClientId { get; set; } = string.Empty;

        /// <summary>
        /// ClientSecret do Google
        /// </summary>
        public string GoogleOpenIdClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// URI de redirecionamento
        /// </summary>
        public string GoogleOpenIdRedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// URL no caso de redirecionamento na aplicação web
        /// </summary>
        public string GoogleOpenIdWebSuccessRedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// State Code do Google
        /// </summary>
        public string GoogleOpenIdStateCode { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação do registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Data de atualização do registro
        /// </summary>
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

        /// <summary>
        /// Configurações iniciais
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerSettings>()
                .ToTable("server_settings")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ServerSettings>()
                .Property(x => x.OpenRouterTemperature)
                .HasPrecision(4, 2);
        }
    }
}
