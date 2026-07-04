using eTasks_server.Models.Enums.Bonus;

namespace eTasks_server.Models.DTOs.Users.Profile.Responses
{
    /// <summary>
    /// DTO com os pontos de bonus do usuário
    /// </summary>
    public class UserBonusPointEntryDTO
    {
        /// <summary>
        /// Identificador
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Pontos
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Fonte originária dos pontos
        /// </summary>
        public BonusPointSource Source { get; set; }

        /// <summary>
        /// Descrição
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Identificador da fonte
        /// </summary>
        public Guid? SourceReferenceId { get; set; }

        /// <summary>
        /// Data/hora da criação
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
