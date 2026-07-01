namespace eTasks_server.Models.DTOs.AI.Responses
{
    /// <summary>
    /// Resposta do payload de um campo de IA, contendo informações sobre o nome, propriedade alvo, descrição e se é obrigatório.
    /// </summary>
    public class AiPayloadFieldResponse
    {
        /// <summary>
        /// Nome do campo de IA.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Propriedade alvo do campo de IA.
        /// </summary>
        public string TargetProperty { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do campo de IA.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se o campo de IA é obrigatório.
        /// </summary>
        public bool Required { get; set; }
    }
}
