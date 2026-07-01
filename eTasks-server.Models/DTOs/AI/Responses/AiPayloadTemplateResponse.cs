namespace eTasks_server.Models.DTOs.AI.Responses
{
    /// <summary>
    /// Template de resposta para a criação de payloads de API com base em prompts de IA.
    /// </summary>
    public class AiPayloadTemplateResponse
    {
        /// <summary>
        /// Padrão de rota da API para a qual o payload será enviado.
        /// </summary>
        public string RoutePattern { get; set; } = string.Empty;

        /// <summary>
        /// Método HTTP a ser usado para enviar o payload (por exemplo, POST, GET, PUT, DELETE).
        /// </summary>
        public string Method { get; set; } = "POST";

        /// <summary>
        /// Padrão de título do recurso sugerido para o payload.
        /// </summary>
        public string SuggestedResourceTitlePattern { get; set; } = string.Empty;
        
        /// <summary>
        /// Padrão de conteúdo do recurso sugerido para o payload.
        /// </summary>
        public string SuggestedResourceContentPattern { get; set; } = string.Empty;
        
        /// <summary>
        /// Padrão de contexto adicional sugerido para o payload.
        /// </summary>
        public string SuggestedAdditionalContextPattern { get; set; } = string.Empty;

        /// <summary>
        /// Campo de dados que representa os campos do payload, incluindo nome, tipo e descrição.
        /// </summary>
        public List<AiPayloadFieldResponse> Fields { get; set; } = [];

        /// <summary>
        /// Lista de prompts de exemplo que podem ser usados para gerar payloads.
        /// </summary>
        public List<string> ExamplePrompts { get; set; } = [];
    }
}
