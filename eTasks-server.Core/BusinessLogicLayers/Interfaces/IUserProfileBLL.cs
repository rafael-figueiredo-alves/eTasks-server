using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IUserProfileBLL
    {
        Task<UserProfileResponse> GetProfileAsync(Guid userUid);
        Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
        Task<UserSettingsDTO> PatchSettingsAsync(Guid userUid, PatchUserSettingsRequest request);
        Task<UserSettingsSyncResponse> GetSettingsAsync(Guid userUid);
        Task<UserBonusSyncResponse> GetBonusAsync(Guid userUid);
        Task<UserDataSyncResponse> SyncUserDataAsync(Guid userUid, SyncUserDataRequest request);
        Task<string> ExportProfileCsvAsync(Guid userUid);
        Task SoftDeleteAsync(Guid userUid);
    }
}
