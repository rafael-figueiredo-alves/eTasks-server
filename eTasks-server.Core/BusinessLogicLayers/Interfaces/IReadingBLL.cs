using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IReadingBLL
    {
        Task<List<ReadingListItemResponse>> ListAsync(Guid userUid, ListReadingsRequest request, CancellationToken cancellationToken = default);
        Task<ReadingDetailsResponse> GetByIdAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default);
        Task<ReadingDetailsResponse> CreateAsync(Guid userUid, CreateReadingRequest request, CancellationToken cancellationToken = default);
        Task<ReadingDetailsResponse> UpdateAsync(Guid userUid, Guid readingId, UpdateReadingRequest request, CancellationToken cancellationToken = default);
        Task<ReadingDetailsResponse> UpdateProgressAsync(Guid userUid, Guid readingId, UpdateReadingProgressRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default);
        Task<ReadingSyncResponse> SyncAsync(Guid userUid, SyncReadingsRequest request, CancellationToken cancellationToken = default);
    }
}
