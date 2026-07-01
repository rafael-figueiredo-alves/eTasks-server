namespace eTasks_server.Models.DTOs.AI.Responses
{
    /// <summary>
    /// Classe que representa a resposta de uso da API de IA, contendo informações sobre o número de tokens utilizados no prompt, na conclusão e o total de tokens.
    /// </summary>
    public class AiUsageResponse
    {
        /// <summary>
        /// Obtém ou define o número de tokens utilizados no prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Obtém ou define o número de tokens utilizados na conclusão.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Obtém ou define o número total de tokens utilizados (prompt + conclusão).
        /// </summary>
        public int TotalTokens { get; set; }
    }
}
