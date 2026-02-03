using eTasks_server.Core.Models;

namespace eTasks_server.Core.BusinessLayers
{
    public class VersionBLL
    {
        public static eTasksVersion GetVersion()
        {
            return eTasksVersion.GetCurrentVersion();
        }
    }
}
