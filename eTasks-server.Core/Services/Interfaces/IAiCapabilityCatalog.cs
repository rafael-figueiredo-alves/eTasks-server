using eTasks_server.Models.DTOs.AI.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IAiCapabilityCatalog
    {
        AiCapabilitiesResponse GetCapabilities();
    }
}
