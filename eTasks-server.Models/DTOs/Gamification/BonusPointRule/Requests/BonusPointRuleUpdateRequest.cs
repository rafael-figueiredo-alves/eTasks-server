using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests
{
    /// <summary>
    /// DTO de requisição para atualização de uma regra de bônus existente.
    /// O campo <c>Source</c> é omitido intencionalmente — não pode ser alterado
    /// após a criação da regra, pois impactaria lançamentos já registrados no sistema.
    /// </summary>
    public class BonusPointRuleUpdateRequest
    {
        /// <summary>
        /// Nome amigável da regra.
        /// </summary>
        [Required(ErrorMessage = "O nome da regra é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição resumida da finalidade da regra.
        /// </summary>
        [MaxLength(255, ErrorMessage = "A descrição não pode exceder 255 caracteres.")]
        public string? Description { get; set; }

        /// <summary>
        /// Quantidade padrão de pontos atribuída quando essa origem ocorre.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Os pontos padrão devem ser maiores que zero.")]
        public int DefaultPoints { get; set; }

        /// <summary>
        /// Indica se a pontuação pode ser sobrescrita em lançamentos específicos.
        /// </summary>
        public bool AllowCustomPoints { get; set; }

        /// <summary>
        /// Indica se a regra está ativa e sendo aplicada.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
