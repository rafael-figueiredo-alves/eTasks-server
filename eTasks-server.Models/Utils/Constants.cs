namespace eTasks_server.Models.Utils
{
    public static class Constants
    {
        public const string CorsPolicyName = "WASMAppPolicy";
        public const string AllowedOrigin = "https://rafael-figueiredo-alves.github.io";
        public const string DatabaseConnection = "DefaultConnection";
        public const string ApiBaseUrl = "ApiSettings:BaseUrl";
        public const string HealthCheckEndpoint = "/health";
        public const string URLClientServicesAPISegment = "api/v2/";
        public const string ServerVersion = "0.0.1 (Alpha)";
        public const string ApiVersion = "v2";
        public const string AppTitle = "eTasks Server";
        public const string ApiDescription = "Documentação interativa da API do eTasks Server, gerenciamento de tarefas eficientes.";
        public const string DeveloperName = "Rafael Figueiredo Alves";
        
        public const string OpenApiEndpoint = "openapi/v2.json";
        public const string ScalarDocEndpoint = "docs";
        
        public const string JwtKeyConfig = "Jwt:Key";
        public const string JwtIssuerConfig = "Jwt:Issuer";
        public const string JwtAudienceConfig = "Jwt:Audience";
        
        // SMTP Configurations
        public const string SmtpEnabled = "Smtp:Enabled";
        public const string SmtpHost = "Smtp:Host";
        public const string SmtpPort = "Smtp:Port";
        public const string SmtpEnableSsl = "Smtp:EnableSsl";
        public const string SmtpUsername = "Smtp:Username";
        public const string SmtpPassword = "Smtp:Password";
        public const string SmtpFromEmail = "Smtp:FromEmail";
        public const string SmtpFromName = "Smtp:FromName";
    }
}
