namespace eTasks_server.Models.DTOs.AI.Responses
{
    /// <summary>
    /// Resposta da assistente de IA, incluindo informações sobre o provedor, modelo, conteúdo gerado e uso de tokens.
    /// </summary>
    public class AiAssistResponse
    {
        /// <summary>
        /// Provedor de IA utilizado para gerar a resposta (ex: OpenAI, Azure, etc.).
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Modelo de IA utilizado para gerar a resposta (ex: GPT-4, GPT-3.5, etc.).
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo gerado pela assistente de IA em resposta à solicitação do usuário.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Informações sobre o uso de tokens durante a geração da resposta, incluindo contagem de tokens e limites.
        /// </summary>
        public AiUsageResponse Usage { get; set; } = new();
    }
}
