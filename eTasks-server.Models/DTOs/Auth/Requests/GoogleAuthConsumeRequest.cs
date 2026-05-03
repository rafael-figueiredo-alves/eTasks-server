using eTasks_server.Models.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    public class GoogleAuthConsumeRequest
    {
        [Required]
        public Guid SessionCode { get; set; }

        [Required]
        [AllowedUserAgent]
        public string UserAgent { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(120)]
        public string ClientInstanceId { get; set; } = string.Empty;
    }
}
