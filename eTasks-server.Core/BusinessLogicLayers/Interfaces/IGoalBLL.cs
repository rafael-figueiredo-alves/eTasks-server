using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IGoalBLL
    {
        Task<List<GoalListItemResponse>> ListAsync(Guid userUid, ListGoalsRequest request, CancellationToken cancellationToken = default);
        Task<GoalDetailsResponse> GetByIdAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default);
        Task<GoalDetailsResponse> CreateAsync(Guid userUid, CreateGoalRequest request, CancellationToken cancellationToken = default);
        Task<GoalDetailsResponse> UpdateAsync(Guid userUid, Guid goalId, UpdateGoalRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userUid, Guid goalId, CancellationToken cancellationToken = default);
        Task<GoalSyncResponse> SyncAsync(Guid userUid, SyncGoalsRequest request, CancellationToken cancellationToken = default);
    }
}
