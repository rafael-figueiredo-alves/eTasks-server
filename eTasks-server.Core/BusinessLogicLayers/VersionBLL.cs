using eTasks_server.Core.Data;
using eTasks_server.Models.Version;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.BusinessLayers
{
    public class VersionBLL
    {
        public static async Task<eTasksVersion> GetVersionAsync(AppDbContext dbContext)
        {
            return await dbContext.DbVersion.FirstAsync();
        }

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
