using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.Entities.Version;

namespace eTasks_server.Client.Services
{
    public class VersionService(IVersionBLL _versionBLL) : IVersionService
    {
        /// <summary>
        /// Obtem a versão mais recente do aplicativo. Se não houver nenhuma versão salva, retorna null.
        /// </summary>
        /// <returns></returns>
        public async Task<eTasksVersion> GetVersionAsync()
        {
            return await _versionBLL.GetVersionAsync();
        }

        /// <summary>
        /// Salva uma nova versão do aplicativo. Retorna true se a operação for bem-sucedida, ou false caso contrário.
        /// </summary>
        /// <param name="version">Dados da versão</param>
        /// <returns></returns>
        public async Task<bool> SaveVersionAsync(eTasksVersion version)
        {
            return await _versionBLL.SaveNewVersionAsync(version);
        }
    }
}
