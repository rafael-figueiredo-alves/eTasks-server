using eTasks_server.Core.Data;
using eTasks_server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.BusinessLayers
{
    public class VersionBLL
    {
        public static async Task<eTasksVersion> GetVersionAsync(AppDbContext dbContext)
        {
            //return eTasksVersion.GetCurrentVersion();

            return await dbContext.DbVersion.FirstOrDefaultAsync();
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
