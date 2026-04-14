using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.Entities.Version;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers
{
    /// <summary>
    /// Regras de negócio relacionadas à versão do aplicativo eTasks (Mobile e Web).
    /// </summary>
    public class VersionBLL : BaseBLL<IVersionBLL>, IVersionBLL
    {
        public VersionBLL(AppDbContext context, ILogger<IVersionBLL> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// Retornar informaões sobre a versão atual do aplicativo eTasks (Mobile e Web) armazenada no banco de dados.
        /// </summary>
        /// <returns>Objeto eTasksVersion</returns>
        public async Task<eTasksVersion> GetVersionAsync()
        {
            return await _context.DbVersion.OrderBy(v => v.Id).FirstAsync();
        }

        /// <summary>
        /// Salva alterações na versão do aplicativo eTasks (Mobile e Web) no banco de dados. Retorna true se a operação for bem-sucedida, ou false em caso de falha.
        /// </summary>
        /// <param name="version">objeto eTasksVerion</param>
        /// <returns>True/False</returns>
        public async Task<bool> SaveNewVersionAsync(eTasksVersion version)
        {
            try
            {
                _context.DbVersion.Update(version);
                await SaveChangesContextAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
