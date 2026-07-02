namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Representa uma requisição de sincronização de item de nota, incluindo informações sobre a operação a ser realizada (criação ou atualização), o ID da nota, o ETag esperado e os detalhes da nota a ser criada ou atualizada.
    /// </summary>
    public class NotePushSyncItemRequest
    {
        /// <summary>
        /// Identificador único da mutação do cliente, utilizado para rastrear a operação de sincronização.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de operação a ser realizada (criação, atualização ou exclusão).
        /// </summary>
        public NotePushOperationType Operation { get; set; }

        /// <summary>
        /// Identificador único da nota a ser atualizada ou excluída. Pode ser nulo se a operação for de criação.
        /// </summary>
        public Guid? NoteId { get; set; }

        /// <summary>
        /// ETag esperado da nota, utilizado para controle de concorrência otimista. Pode ser nulo se a operação for de criação.
        /// </summary>
        public string? ExpectedEtag { get; set; }

        /// <summary>
        /// Detalhes da nota a ser criada. Pode ser nulo se a operação não for de criação.
        /// </summary>
        public CreateNoteRequest? Create { get; set; }

        /// <summary>
        /// Detalhes da nota a ser atualizada. Pode ser nulo se a operação não for de atualização.
        /// </summary>
        public UpdateNoteRequest? Update { get; set; }
    }
}
