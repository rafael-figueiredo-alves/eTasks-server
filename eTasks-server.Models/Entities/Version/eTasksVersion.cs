using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.Entities.Version
{
    public class eTasksVersion
    {
        [AllowedValues([1])]
        public int Id { get; set; } = 1;

        [Required]
        public int AppVersion { get; set; } = 1;

        [Required]
        public string DisplayVersion { get; set; } = "2.0.0";

        public string URL_APK { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.apk";
        public string URL_Win { get; set; } = "https://github.com/rafael-figueiredo-alves/eTasks/releases/download/v1.1/eTasks.exe";
    }
}
