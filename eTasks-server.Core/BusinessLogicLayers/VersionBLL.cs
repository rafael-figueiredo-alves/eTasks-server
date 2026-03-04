using eTasks_server.Core.Data;
using eTasks_server.Models.Version;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.BusinessLayers
{
    /// <summary>
    /// Regras de negócio relacionadas à versão do aplicativo eTasks (Mobile e Web).
    /// </summary>
    public class VersionBLL
    {
        /// <summary>
        /// Retornar informaões sobre a versão atual do aplicativo eTasks (Mobile e Web) armazenada no banco de dados.
        /// </summary>
        /// <param name="dbContext">Contexto do banco de dados</param>
        /// <returns>Objeto eTasksVersion</returns>
        public static async Task<eTasksVersion> GetVersionAsync(AppDbContext dbContext)
        {
            return await dbContext.DbVersion.OrderBy(v => v.Id).FirstAsync();
        }

        /// <summary>
        /// Salva alterações na versão do aplicativo eTasks (Mobile e Web) no banco de dados. Retorna true se a operação for bem-sucedida, ou false em caso de falha.
        /// </summary>
        /// <param name="dbContext">Contexto de dados</param>
        /// <param name="version">objeto eTasksVerion</param>
        /// <returns>True/False</returns>
        public static async Task<bool> SaveNewVersionAsync(AppDbContext dbContext, eTasksVersion version)
        {
            try
            {
                dbContext.DbVersion.Update(version);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
