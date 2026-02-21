using eTasks_server.Core.Data;
using eTasks_server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace eTasks_server.Core.BusinessLayers
{
    public class VersionBLL
    {
        public static async  Task<eTasksVersion> GetVersion(AppDbContext dbContext)
        {
            //return eTasksVersion.GetCurrentVersion();

            return await dbContext.DbVersion.FirstOrDefaultAsync();
        }
    }
}
