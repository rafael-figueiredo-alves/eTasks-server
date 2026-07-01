using eTasks_server.Models.Enums.Finances;

namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Representa retorno de uma tentativa de sincronização de um item de lançamento financeiro.
    /// </summary>
    public class FinanceEntryPushSyncItemResponse
    {
        /// <summary>
        /// Identificador único da mutação do cliente, utilizado para rastrear a solicitação de sincronização.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Status da tentativa de sincronização do item de lançamento financeiro.
        /// </summary>
        public FinanceEntryPushSyncItemStatus Status { get; set; }

        /// <summary>
        /// Detalhes do lançamento financeiro sincronizado, caso a sincronização tenha sido bem-sucedida.
        /// </summary>
        public FinanceEntryDetailsResponse? FinanceEntry { get; set; }

        /// <summary>
        /// Informações sobre a entrada financeira excluída, caso a sincronização tenha resultado em uma exclusão.
        /// </summary>
        public DeletedFinanceEntryResponse? Deleted { get; set; }

        /// <summary>
        /// Etag do servidor para o item de lançamento financeiro sincronizado, utilizado para controle de versão e cache.
        /// </summary>
        public string? ServerEtag { get; set; }

        /// <summary>
        /// Código de erro retornado pelo servidor em caso de falha na sincronização, caso aplicável.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Mensagem de erro detalhada retornada pelo servidor em caso de falha na sincronização, caso aplicável.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
