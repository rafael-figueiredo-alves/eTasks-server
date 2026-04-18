using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;

namespace eTasks_server.Client.Services
{
    public class UserProfileService(IUserProfileBLL _userProfileBLL) : IUserProfileService
    {
        public Task<UserProfileResponse> GetProfileAsync(Guid userUid)
        {
            return _userProfileBLL.GetProfileAsync(userUid);
        }

        public Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request)
        {
            return _userProfileBLL.UpdateProfileAsync(userUid, request);
        }

        public Task<UserSettingsDTO> UpdateSettingsAsync(Guid userUid, PatchUserSettingsRequest request)
        {
            return _userProfileBLL.PatchSettingsAsync(userUid, request);
        }
    }
}
