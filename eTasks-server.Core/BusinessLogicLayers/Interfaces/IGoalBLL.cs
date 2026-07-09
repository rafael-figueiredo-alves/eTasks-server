using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface do recurso de objetivos
    /// </summary>
    public interface IGoalBLL
    {
        /// <summary>
        /// Obtem lista de objetivos
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<GoalListItemResponse>> ListAsync(Guid userUid, ListGoalsRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem um objetivo por id
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="goalId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GoalDetailsResponse> GetByIdAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria um novo objetivo/meta
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GoalDetailsResponse> CreateAsync(Guid userUid, CreateGoalRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza uma meta/objetivo
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="goalId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GoalDetailsResponse> UpdateAsync(Guid userUid, Guid goalId, UpdateGoalRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exclui objetivo/meta
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="goalId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza dados
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GoalSyncResponse> SyncAsync(Guid userUid, SyncGoalsRequest request, CancellationToken cancellationToken = default);
    }
}
