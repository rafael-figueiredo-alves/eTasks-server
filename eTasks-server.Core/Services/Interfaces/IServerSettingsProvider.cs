using eTasks_server.Models.Entities.Settings;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface para fornecer configurações do servidor.
    /// </summary>
    public interface IServerSettingsProvider
    {
        /// <summary>        
        /// Obtém as configurações atuais do servidor.       
        /// </summary>
        Task<ServerSettings> GetCurrentAsync(CancellationToken cancellationToken = default);
    }
}
