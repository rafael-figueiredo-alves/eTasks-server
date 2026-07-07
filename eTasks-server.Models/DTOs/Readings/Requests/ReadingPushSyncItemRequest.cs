using eTasks_server.Models.Enums.Readings;

namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// Requisição para sincronizar leitura
    /// </summary>
    public class ReadingPushSyncItemRequest
    {
        /// <summary>
        /// Id do item de leitura no cliente
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Operação de sincronização
        /// </summary>
        public ReadingPushOperationType Operation { get; set; }

        /// <summary>
        /// Id do item de leitura
        /// </summary>
        public Guid? ReadingId { get; set; }

        /// <summary>
        /// E-Tag esperada
        /// </summary>
        public string? ExpectedEtag { get; set; }

        /// <summary>
        /// Requisição de criação
        /// </summary>
        public CreateReadingRequest? Create { get; set; }

        /// <summary>
        /// Requisição para atualizar
        /// </summary>
        public UpdateReadingRequest? Update { get; set; }

        /// <summary>
        /// Requisição para atualizar progresso de leitura
        /// </summary>
        public UpdateReadingProgressRequest? Progress { get; set; }
    }
}
