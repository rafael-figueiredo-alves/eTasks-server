using eTasks_server.Models.Enums.Ai;

namespace eTasks_server.Models.DTOs.AI.Requests
{
    /// <summary>
    /// Representa a requisição de assistência de IA, incluindo o tipo de recurso, intenção da interação, prompt do usuário, título e conteúdo do recurso, contexto adicional e histórico de conversas.
    /// </summary>
    public class AiAssistRequest
    {
        /// <summary>
        /// Obtém ou define o tipo de recurso para o qual a assistência de IA é solicitada.
        /// </summary>
        public AiResourceType Resource { get; set; } = AiResourceType.General;
        
        /// <summary>
        /// Obtém ou define a intenção da interação.
        /// </summary>
        public AiInteractionIntent Intent { get; set; } = AiInteractionIntent.GeneralHelp;
        
        /// <summary>
        /// Obtém ou define o prompt do usuário.
        /// </summary>
        public string UserPrompt { get; set; } = string.Empty;
        
        /// <summary>
        /// Obtém ou define o título do recurso.
        /// </summary>
        public string? ResourceTitle { get; set; }
        
        /// <summary>
        /// Obtém ou define o conteúdo do recurso.
        /// </summary>
        public string? ResourceContent { get; set; }
        
        /// <summary>
        /// Obtém ou define o contexto adicional.
        /// </summary>
        public string? AdditionalContext { get; set; }
        
        /// <summary>
        /// Obtém ou define o histórico de conversas.
        /// </summary>
        public List<AiConversationMessageRequest> Conversation { get; set; } = [];
    }
}
