using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
{
    
}

namespace eTasks_server.Core.BusinessLayers
{
    public class VersionBLL
    {
        public static Results GetVersion()
        {
            var version = Models.Version.GetCurrentVersion();
            return Results.Ok(new
            {
                version.AppVersion,
                version.DisplayVersion,
                version.URL_APK,
                version.URL_Win
            });
        }
    }
}
