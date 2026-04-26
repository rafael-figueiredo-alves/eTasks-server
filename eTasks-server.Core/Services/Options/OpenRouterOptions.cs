namespace eTasks_server.Core.Services.Options
{
    public class OpenRouterOptions
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/";
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "openrouter/auto";
        public string? SiteUrl { get; set; }
        public string? AppName { get; set; }
        public decimal Temperature { get; set; } = 0.3m;
        public int MaxTokens { get; set; } = 700;
    }
}
