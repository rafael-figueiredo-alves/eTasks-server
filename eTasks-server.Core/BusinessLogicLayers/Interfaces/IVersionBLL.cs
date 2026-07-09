using eTasks_server.Models.Entities.Version;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de versão
    /// </summary>
    public interface IVersionBLL
    {
        /// <summary>
        /// Obtem versão
        /// </summary>
        /// <returns></returns>
        Task<eTasksVersion> GetVersionAsync();

        /// <summary>
        /// Salva dados da nova versão
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<bool> SaveNewVersionAsync(eTasksVersion version);
    }
}
