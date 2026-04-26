using eTasks_server.Models.DTOs.AI.Requests;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IAiPromptComposer
    {
        string BuildSystemPrompt(AiAssistRequest request);
        string BuildUserPrompt(AiAssistRequest request);
    }
}
