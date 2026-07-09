using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface do recurso de gerencionamento de finanças
    /// </summary>
    public interface IFinanceBLL
    {
        /// <summary>
        /// Lista entradas financeiras
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<FinanceEntryListItemResponse>> ListAsync(Guid userUid, ListFinanceEntriesRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem uma entrada pelo ID informado
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="financeEntryId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FinanceEntryDetailsResponse> GetByIdAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria uma entrada financeira
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FinanceEntryDetailsResponse> CreateAsync(Guid userUid, CreateFinanceEntryRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza uma entrada financeira
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="financeEntryId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FinanceEntryDetailsResponse> UpdateAsync(Guid userUid, Guid financeEntryId, UpdateFinanceEntryRequest request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Exclui entrada financeira
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="financeEntryId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem resumo mensal
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FinanceMonthSummaryResponse> GetMonthSummaryAsync(Guid userUid, int year, int month, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza dados financeiras (processo offline first)
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<FinanceEntrySyncResponse> SyncAsync(Guid userUid, SyncFinanceEntriesRequest request, CancellationToken cancellationToken = default);
    }
}
