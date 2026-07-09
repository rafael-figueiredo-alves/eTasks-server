using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de perfil de usuário
    /// </summary>
    public interface IUserProfileBLL
    {
        /// <summary>
        /// Obtem perfil
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        Task<UserProfileResponse> GetProfileAsync(Guid userUid);

        /// <summary>
        /// Atualiza o perfil
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza as configurações do usuário
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserSettingsDTO> PatchSettingsAsync(Guid userUid, PatchUserSettingsRequest request);

        /// <summary>
        /// Obtem as configurações
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        Task<UserSettingsSyncResponse> GetSettingsAsync(Guid userUid);

        /// <summary>
        /// Obtem os bonus
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        Task<UserBonusSyncResponse> GetBonusAsync(Guid userUid);

        /// <summary>
        /// Sincroniza os dados do usuário
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<UserDataSyncResponse> SyncUserDataAsync(Guid userUid, SyncUserDataRequest request);

        /// <summary>
        /// Exporta dados do usuário para CSV
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        Task<string> ExportProfileCsvAsync(Guid userUid);

        /// <summary>
        /// Marca conta para exclusão
        /// </summary>
        /// <param name="userUid"></param>
        /// <returns></returns>
        Task SoftDeleteAsync(Guid userUid);
    }
}
