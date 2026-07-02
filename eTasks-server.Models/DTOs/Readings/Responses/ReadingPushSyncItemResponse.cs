using eTasks_server.Models.Enums.Readings;

namespace eTasks_server.Models.DTOs.Readings.Responses
{
    /// <summary>
    /// Representa a resposta de um item de sincronização de leitura enviado para o servidor.
    /// </summary>
    public class ReadingPushSyncItemResponse
    {
        /// <summary>
        /// Identificador único da mutação do cliente, usado para rastrear a solicitação de sincronização.
        /// </summary>
        public string ClientMutationId { get; set; } = string.Empty;

        /// <summary>
        /// Status da operação de sincronização do item de leitura, indicando se foi bem-sucedida, falhou ou se o item foi excluído.
        /// </summary>
        public ReadingPushSyncItemStatus Status { get; set; }

        /// <summary>
        /// Detalhes da leitura sincronizada, caso a operação tenha sido bem-sucedida.
        /// </summary>
        public ReadingDetailsResponse? Reading { get; set; }

        /// <summary>
        /// Detalhes da leitura excluída, caso a operação tenha sido bem-sucedida e o item tenha sido removido.
        /// </summary>
        public DeletedReadingResponse? Deleted { get; set; }

        /// <summary>
        /// Etag do servidor para o item de leitura sincronizado, usado para controle de versão e cache.
        /// </summary>
        public string? ServerEtag { get; set; }

        /// <summary>
        /// Código de erro retornado pelo servidor em caso de falha na operação de sincronização, útil para diagnóstico e tratamento de erros.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Mensagem de erro detalhada fornecida pelo servidor em caso de falha na operação de sincronização, útil para exibição ao usuário ou registro de logs.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
