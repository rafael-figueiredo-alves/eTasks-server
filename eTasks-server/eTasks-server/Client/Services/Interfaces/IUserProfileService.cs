using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileResponse> GetProfileAsync(Guid userUid);
        Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request);
        Task<UserSettingsDTO> UpdateSettingsAsync(Guid userUid, PatchUserSettingsRequest request);
    }
}
