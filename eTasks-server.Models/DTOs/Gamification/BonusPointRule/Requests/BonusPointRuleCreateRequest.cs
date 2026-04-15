using System.ComponentModel.DataAnnotations;

namespace eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests
{
    /// <summary>
    /// DTO de requisição para criação de uma regra de bônus.
    /// Inclui o campo <see cref="Source"/> que define a origem dos pontos
    /// e não poderá ser alterado após a criação.
    /// </summary>
    public class BonusPointRuleCreateRequest
    {
        /// <summary>
        /// Origem do ponto coberta pela regra (valor inteiro do enum BonusPointSource).
        /// Este campo é imutável após a criação da regra.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "O valor de Source deve ser válido.")]
        public int Source { get; set; }

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
        public bool AllowCustomPoints { get; set; } = true;

        /// <summary>
        /// Indica se a regra deve estar ativa ao ser criada.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
