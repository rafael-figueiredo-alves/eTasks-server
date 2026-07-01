using eTasks_server.Models.Enums.Goals;

namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Representa uma solicitação de sincronização de metas entre o cliente e o servidor.
    /// </summary>
    public class GoalPushSyncItemRequest
    {
        /// <summary>
        /// Identificador único da mutação do cliente, usado para rastrear a solicitação de sincronização.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de operação a ser realizada na meta (criação, atualização ou exclusão).
        /// </summary>
        public GoalPushOperationType Operation { get; set; }

        /// <summary>
        /// Identificador da meta a ser atualizada ou excluída. Deve ser fornecido para operações de atualização e exclusão.
        /// </summary>
        public Guid? GoalId { get; set; }

        /// <summary>
        /// Etag esperado da meta para operações de atualização e exclusão. Se fornecido, o servidor verificará se o Etag corresponde ao da meta existente antes de realizar a operação.
        /// </summary>
        public string? ExpectedEtag { get; set; }

        /// <summary>
        /// Dados para criar uma nova meta. Deve ser fornecido para operações de criação.
        /// </summary>
        public CreateGoalRequest? Create { get; set; }

        /// <summary>
        /// Dados para atualizar uma meta existente. Deve ser fornecido para operações de atualização.
        /// </summary>
        public UpdateGoalRequest? Update { get; set; }
    }
}
