using eTasks_server.Models.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Users.Profile.Requests
{
    public class UpdateUserProfileRequest
    {
        [Required]
        [MinLength(3, ErrorMessage = "O nome do usuario precisa conter pelo menos 3 caracteres.")]
        [MaxLength(30, ErrorMessage = "O nome do usuario nao pode exceder 30 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "So e aceito endereco de e-mail valido.")]
        public string Email { get; set; } = string.Empty;

        [Base64String(ErrorMessage = "Formato de imagem aceito e apenas Base64.")]
        public string? PhotoBase64 { get; set; }

        public bool RemovePhoto { get; set; }
    }
}
