using eTasks_server.Models.Entities.Version;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Serviço para lidar com operações relacionadas à versão do aplicativo, como obtenção e atualização da versão atual.
    /// </summary>
    public interface IVersionService
    {
        /// <summary>
        /// Obtém a versão atual do aplicativo, incluindo informações como número da versão, URL para download, etc.
        /// </summary>
        /// <returns></returns>
        Task<eTasksVersion> GetVersionAsync();

        /// <summary>
        /// Salva ou atualiza a versão do aplicativo. Isso pode ser usado para atualizar as informações de versão, como número da versão, URL de download, etc.
        /// </summary>
        /// <param name="version">Informações da versão do aplicativo</param>
        /// <returns>Retorna true se a operação for bem-sucedida, caso contrário, false</returns>
        Task<bool> SaveVersionAsync(eTasksVersion version);
    }
}
