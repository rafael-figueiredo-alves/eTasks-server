using eTasks_server.Models.DTOs.AI.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface que define os métodos para acessar o catálogo de capacidades de IA.
    /// </summary>
    public interface IAiCapabilityCatalog
    {
        /// <summary>
        /// Obtém as capacidades de IA disponíveis no catálogo.
        /// </summary>
        /// <returns></returns>
        AiCapabilitiesResponse GetCapabilities();
    }
}
