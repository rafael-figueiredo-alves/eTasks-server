using eTasks_server.Models.Enums.Ai;

namespace eTasks_server.Models.DTOs.AI.Responses
{
    /// <summary>
    /// Resposta que descreve as capacidades de um recurso de IA, incluindo seu tipo, rótulo, usos recomendados, intenções suportadas, diretrizes e modelo de payload.
    /// </summary>
    public class AiResourceCapabilityResponse
    {
        /// <summary>
        /// Recurso de IA associado a esta resposta de capacidade.
        /// </summary>
        public AiResourceType Resource { get; set; }

        /// <summary>
        /// Rotulo ou nome do recurso de IA, fornecendo uma descrição legível para humanos.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Lista de usos recomendados para o recurso de IA, indicando como ele pode ser aplicado de maneira eficaz.
        /// </summary>
        public List<string> RecommendedUses { get; set; } = [];

        /// <summary>
        /// Lista de intenções suportadas pelo recurso de IA, descrevendo os tipos de tarefas ou interações que ele pode realizar.
        /// </summary>
        public List<string> SupportedIntents { get; set; } = [];

        /// <summary>
        /// Lista de diretrizes ou restrições que devem ser seguidas ao utilizar o recurso de IA, garantindo o uso responsável e seguro.
        /// </summary>
        public List<string> Guardrails { get; set; } = [];

        /// <summary>
        /// Modelo de payload que descreve a estrutura de dados esperada para interagir com o recurso de IA, incluindo os campos necessários e seus tipos.
        /// </summary>
        public AiPayloadTemplateResponse PayloadTemplate { get; set; } = new();
    }
}
