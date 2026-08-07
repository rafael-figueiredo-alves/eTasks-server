using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.DTOs.AI.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço de assistente de IA, definindo o contrato para a funcionalidade de assistência baseada em IA.
    /// </summary>
    public interface IAiAssistantService
    {
        /// <summary>
        /// Fornece assistência baseada em IA com base no prompt do usuário, contexto adicional e histórico de conversas.
        /// </summary>
        /// <param name="userUid">O UID do usuário</param>
        /// <param name="request">A solicitação de assistência de IA</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns></returns>
        Task<AiAssistResponse> AssistAsync(Guid userUid, AiAssistRequest request, CancellationToken cancellationToken = default);
    }
}
