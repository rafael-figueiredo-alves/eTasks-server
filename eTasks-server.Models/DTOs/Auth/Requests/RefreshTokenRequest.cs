using eTasks_server.Models.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Auth.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

        [Required]
        [AllowedUserAgent]
        public string? UserAgent { get; set; }
    }
}
