namespace eTasks_server.Models.DTOs.AI.Requests
{   
    /// <summary>
    /// Representa a requisição de uma mensagem em uma conversa com a API de IA.
    /// </summary>
    public class AiConversationMessageRequest
    {
        /// <summary>
        /// Papel do remetente da mensagem (ex: "user", "assistant", "system").
        /// </summary>
        public string Role { get; set; } = "user";

        /// <summary>
        /// Conteúdo da mensagem enviada na conversa.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
