using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IFinanceBLL
    {
        Task<List<FinanceEntryListItemResponse>> ListAsync(Guid userUid, ListFinanceEntriesRequest request, CancellationToken cancellationToken = default);
        Task<FinanceEntryDetailsResponse> GetByIdAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default);
        Task<FinanceEntryDetailsResponse> CreateAsync(Guid userUid, CreateFinanceEntryRequest request, CancellationToken cancellationToken = default);
        Task<FinanceEntryDetailsResponse> UpdateAsync(Guid userUid, Guid financeEntryId, UpdateFinanceEntryRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default);
        Task<FinanceMonthSummaryResponse> GetMonthSummaryAsync(Guid userUid, int year, int month, CancellationToken cancellationToken = default);
        Task<FinanceEntrySyncResponse> SyncAsync(Guid userUid, SyncFinanceEntriesRequest request, CancellationToken cancellationToken = default);
    }
}
