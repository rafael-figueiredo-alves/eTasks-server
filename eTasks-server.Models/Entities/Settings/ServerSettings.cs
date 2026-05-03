using eTasks_server.Models.Entities;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Models.Entities.Settings
{
    public class ServerSettings : IEntityModelConfiguration<ServerSettings>
    {
        public int Id { get; set; } = 1;

        public bool SmtpEnabled { get; set; }
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool SmtpEnableSsl { get; set; } = true;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SmtpFromEmail { get; set; } = string.Empty;
        public string SmtpFromName { get; set; } = string.Empty;

        public bool OpenRouterEnabled { get; set; }
        public string OpenRouterBaseUrl { get; set; } = "https://openrouter.ai/api/v1/";
        public string OpenRouterApiKey { get; set; } = string.Empty;
        public string OpenRouterModel { get; set; } = "meta-llama/llama-3.3-8b-instruct:free";
        public string OpenRouterSiteUrl { get; set; } = string.Empty;
        public string OpenRouterAppName { get; set; } = "eTasks Server";
        public decimal OpenRouterTemperature { get; set; } = 0.3m;
        public int OpenRouterMaxTokens { get; set; } = 700;

        public bool MongoAuditEnabled { get; set; }
        public string MongoAuditConnectionString { get; set; } = string.Empty;
        public string MongoAuditDatabaseName { get; set; } = "etasks_server";
        public string MongoAuditCollectionName { get; set; } = "operation_audit_logs";
        public int ApplicationLogRetentionDays { get; set; } = 7;

        public bool GoogleOpenIdEnabled { get; set; }
        public string GoogleOpenIdClientId { get; set; } = string.Empty;
        public string GoogleOpenIdClientSecret { get; set; } = string.Empty;
        public string GoogleOpenIdRedirectUri { get; set; } = string.Empty;
        public string GoogleOpenIdWebSuccessRedirectUrl { get; set; } = string.Empty;
        public string GoogleOpenIdStateCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();
        public DateTime UpdatedAt { get; set; } = SaoPauloDateTime.Now();

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
