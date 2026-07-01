namespace eTasks_server.Models.DTOs.AI.Responses
{
    /// <summary>
    /// Resposta que descreve as capacidades da assistente de IA, incluindo o modo do provedor, orientações transversais e recursos disponíveis.
    /// </summary>
    public class AiCapabilitiesResponse
    {
        /// <summary>
        /// Modo do provedor de IA utilizado para gerar a resposta (ex: OpenRouter, Azure, etc.).
        /// </summary>
        public string ProviderMode { get; set; } = "OpenRouter";

        /// <summary>
        /// Orientações transversais fornecidas pela assistente de IA, que podem incluir recomendações, melhores práticas ou diretrizes para o uso da IA.
        /// </summary>
        public List<string> CrossCuttingGuidance { get; set; } = [];

        /// <summary>
        /// Recursos disponíveis na assistente de IA, incluindo informações sobre cada recurso, como nome, descrição e capacidades específicas.
        /// </summary>
        public List<AiResourceCapabilityResponse> Resources { get; set; } = [];
    }
}
