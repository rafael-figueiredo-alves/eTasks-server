using eTasks_server.Models.Enums.Finances;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Requesição de sincronização de item de lançamento financeiro.
    /// </summary>
    public class FinanceEntryPushSyncItemRequest
    {
        /// <summary>
        /// Identificador único da mutação do cliente para rastrear a solicitação.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Operação a ser realizada no lançamento financeiro (criação ou atualização).
        /// </summary>
        public FinanceEntryPushOperationType Operation { get; set; }

        /// <summary>
        /// Identificador único do lançamento financeiro a ser atualizado (apenas para operações de atualização).
        /// </summary>
        public Guid? FinanceEntryId { get; set; }

        /// <summary>
        /// Etag esperado do lançamento financeiro para garantir a consistência de dados (apenas para operações de atualização).
        /// </summary>
        public string? ExpectedEtag { get; set; }

        /// <summary>
        /// Dados do lançamento financeiro a ser criado (apenas para operações de criação).
        /// </summary>
        public CreateFinanceEntryRequest? Create { get; set; }

        /// <summary>
        /// Dados do lançamento financeiro a ser atualizado (apenas para operações de atualização).
        /// </summary>
        public UpdateFinanceEntryRequest? Update { get; set; }
    }
}
