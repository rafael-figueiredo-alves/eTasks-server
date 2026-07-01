using eTasks_server.Models.Enums.Goals;

namespace eTasks_server.Models.DTOs.Goals.Responses
{
    /// <summary>
    /// Retorno da resposta do endpoint de sincronização de metas (GoalPushSyncItem).
    /// </summary>
    public class GoalPushSyncItemResponse
    {
        /// <summary>
        /// Identificador único da mutação do cliente, utilizado para correlacionar a solicitação e a resposta.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Status da operação de sincronização da meta.
        /// </summary>
        public GoalPushSyncItemStatus Status { get; set; }

        /// <summary>
        /// Código de erro, caso a operação tenha falhado. Pode ser nulo se a operação foi bem-sucedida.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Mensagem de erro detalhada, caso a operação tenha falhado. Pode ser nula se a operação foi bem-sucedida.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Detalhes da meta sincronizada, caso a operação tenha sido bem-sucedida. Pode ser nulo se a operação falhou ou se a meta foi deletada.
        /// </summary>
        public GoalDetailsResponse? Goal { get; set; }

        /// <summary>
        /// Informações sobre a meta excluída, caso a operação tenha sido bem-sucedida. Pode ser nulo se a operação falhou ou se a meta não foi excluída.
        /// </summary>
        public DeletedGoalResponse? Deleted { get; set; }

        /// <summary>
        /// Etag do servidor para a meta sincronizada, utilizado para controle de versão e cache. Pode ser nulo se a operação falhou ou se a meta foi deletada.
        /// </summary>
        public string? ServerEtag { get; set; }
    }
}
