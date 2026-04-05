using eTasks_server.Models.DataAnnotations;
using eTasks_server.Models.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace eTasks_server.Models.Entities.Users
{
    public class LoginLog
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid? UserUid { get; set; }

        [AllowedValues(["Success", "Failed", "Blocked"], ErrorMessage = "Os unicos status aceitos sao 'Success', 'Failed' e/ou 'Blocked'")]
        public string Status { get; set; } = string.Empty;

        public string? IpAddress { get; set; }

        [AllowedUserAgent]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = SaoPauloDateTime.Now();

        [JsonIgnore]
        public User? User { get; set; }
    }
}
