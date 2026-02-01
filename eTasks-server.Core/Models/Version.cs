using System;
using System.Collections.Generic;
using System.Text;

namespace eTasks_server.Core.Models
{
    public class Version
    {
        public int AppVersion { get; set; } = 1;
        public string DisplayVersion { get; set; } = "2.0.0";
        public string URL_APK { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.apk";
        public string URL_Win { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.exe";

        public static Version GetCurrentVersion()
        {
            return new Version();
        }
    }
}
