using eTasks_server.Models.DTOs.AI.Requests;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface que define os métodos para compor prompts de IA com base em solicitações de assistência de IA.
    /// </summary>
    public interface IAiPromptComposer
    {
        /// <summary>
        /// Constrói o prompt do sistema com base na solicitação de assistência de IA fornecida.
        /// </summary>
        /// <param name="request">A solicitação de assistência de IA</param>
        /// <returns>O prompt do sistema</returns>
        string BuildSystemPrompt(AiAssistRequest request);

        /// <summary>
        /// Constrói o prompt do usuário com base na solicitação de assistência de IA fornecida.
        /// </summary>
        /// <param name="request">A solicitação de assistência de IA</param>
        /// <returns>O prompt do usuário</returns>
        string BuildUserPrompt(AiAssistRequest request);
    }
}
