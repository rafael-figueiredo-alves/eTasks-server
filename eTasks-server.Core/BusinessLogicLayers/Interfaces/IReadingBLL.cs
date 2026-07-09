using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface do recurso de leituras
    /// </summary>
    public interface IReadingBLL
    {
        /// <summary>
        /// Obtem lista de leituras
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<ReadingListItemResponse>> ListAsync(Guid userUid, ListReadingsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retorna uma leitura por ID informado
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="readingId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ReadingDetailsResponse> GetByIdAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria nova leitura
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ReadingDetailsResponse> CreateAsync(Guid userUid, CreateReadingRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza uma leitura
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="readingId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ReadingDetailsResponse> UpdateAsync(Guid userUid, Guid readingId, UpdateReadingRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza o progresso de uma leitura
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="readingId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ReadingDetailsResponse> UpdateProgressAsync(Guid userUid, Guid readingId, UpdateReadingProgressRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// EXclui uma leitura
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="readingId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza dados
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ReadingSyncResponse> SyncAsync(Guid userUid, SyncReadingsRequest request, CancellationToken cancellationToken = default);
    }
}
