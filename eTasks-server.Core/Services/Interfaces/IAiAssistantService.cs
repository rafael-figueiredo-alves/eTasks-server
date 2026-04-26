using eTasks_server.Models.DTOs.AI.Requests;
using eTasks_server.Models.DTOs.AI.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IAiAssistantService
    {
        Task<AiAssistResponse> AssistAsync(Guid userUid, AiAssistRequest request, CancellationToken cancellationToken = default);
    }
}
