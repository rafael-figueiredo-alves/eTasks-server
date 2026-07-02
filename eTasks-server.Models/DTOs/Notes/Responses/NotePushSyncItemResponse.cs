using eTasks_server.Models.Enums.Notes;

namespace eTasks_server.Models.DTOs.Notes.Responses
{
    /// <summary>
    /// Resposta da sincronização de um item de nota.
    /// </summary>
    public class NotePushSyncItemResponse
    {
        /// <summary>
        /// Identificador da mutação do cliente, usado para rastrear a solicitação de sincronização.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Status da sincronização do item de nota.
        /// </summary>
        public NotePushSyncItemStatus Status { get; set; }

        /// <summary>
        /// Código de erro, se houver algum erro durante a sincronização.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Mensagem de erro detalhada, se houver algum erro durante a sincronização.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Detalhes da nota sincronizada, se a sincronização for bem-sucedida.
        /// </summary>
        public NoteDetailsResponse? Note { get; set; }

        /// <summary>
        /// Detalhes da nota excluída, se a sincronização envolver a exclusão de uma nota.
        /// </summary>
        public DeletedNoteResponse? Deleted { get; set; }

        /// <summary>
        /// Etag do servidor, usado para controle de versão e sincronização de dados.
        /// </summary>
        public string? ServerEtag { get; set; }
    }
}
