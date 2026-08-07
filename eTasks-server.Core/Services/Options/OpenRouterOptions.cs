namespace eTasks_server.Core.Services.Options
{
    /// <summary>
    /// Classe que representa as opções de configuração para a integração com o OpenRouter.
    /// </summary>
    public class OpenRouterOptions
    {
        /// <summary>
        /// Indica se a integração com o OpenRouter está habilitada.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// URL base da API do OpenRouter.
        /// </summary>
        public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/";
        
        /// <summary>
        /// Obtém ou define a chave de API para o OpenRouter.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define o modelo a ser utilizado.
        /// </summary>
        public string Model { get; set; } = "openrouter/auto";
        
        /// <summary>
        /// Obtém ou define a URL do site.
        /// </summary>
        public string? SiteUrl { get; set; }
        
        /// <summary>
        /// Obtém ou define o nome do aplicativo.
        /// </summary>
        public string? AppName { get; set; }

        /// <summary>
        /// Obtém ou define a temperatura para a geração de respostas. A temperatura controla a aleatoriedade das respostas geradas pelo modelo. Valores mais baixos resultam em respostas mais determinísticas, enquanto valores mais altos aumentam a diversidade das respostas.
        /// </summary>
        public decimal Temperature { get; set; } = 0.3m;

        /// <summary>
        /// Obtém ou define o número máximo de tokens que podem ser gerados na resposta. Um token pode ser tão curto quanto um único caractere ou tão longo quanto uma palavra inteira (por exemplo, "a" é um token, e "apple" é outro token). O limite de tokens ajuda a controlar o tamanho da resposta gerada pelo modelo.
        /// </summary>
        public int MaxTokens { get; set; } = 700;
    }
}
