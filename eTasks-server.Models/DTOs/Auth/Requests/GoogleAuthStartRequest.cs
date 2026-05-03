using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    public class GoogleAuthStartRequest
    {
        [Required]
        [AllowedUserAgent]
        public string UserAgent { get; set; } = Constants.WebUserAgent;

        [Required]
        [MinLength(8)]
        [MaxLength(120)]
        public string ClientInstanceId { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ReturnUrl { get; set; }
    }
}
