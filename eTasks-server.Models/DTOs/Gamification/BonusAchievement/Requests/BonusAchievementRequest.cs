using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests
{
    /// <summary>
    /// DTO de requisição para criação ou atualização de uma conquista de bônus
    /// no painel administrativo.
    /// </summary>
    public class BonusAchievementRequest
    {
        /// <summary>
        /// Código único da conquista. Deve ser único em todo o sistema.
        /// </summary>
        [Required(ErrorMessage = "O código da conquista é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O código não pode exceder 50 caracteres.")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Nome/título amigável da conquista.
        /// </summary>
        [Required(ErrorMessage = "O nome da conquista é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição opcional com detalhes sobre os critérios da conquista.
        /// </summary>
        [MaxLength(255, ErrorMessage = "A descrição não pode exceder 255 caracteres.")]
        public string? Description { get; set; }

        /// <summary>
        /// Quantidade de pontos necessária para que o usuário alcance esta conquista.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Os pontos necessários devem ser maiores que zero.")]
        public int PointsRequired { get; set; }

        /// <summary>
        /// Tipo de exibição visual da conquista (0 = Troféu, 1 = Medalha).
        /// </summary>
        public int DisplayType { get; set; }

        /// <summary>
        /// Indica se a conquista deve estar ativa ao ser criada ou atualizada.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
