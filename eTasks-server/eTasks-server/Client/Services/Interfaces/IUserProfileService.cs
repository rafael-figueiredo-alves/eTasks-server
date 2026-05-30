using eTasks_server.Models.DTOs.Users.Profile.Requests;
using eTasks_server.Models.DTOs.Users.Profile.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface de gerenciamento dos dados do perfil do usuário logado (ADM)
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Obter dados do perfil do usuário
        /// </summary>
        /// <param name="userUid">Identificação do usuário</param>
        /// <returns>Perfil do usuário</returns>
        Task<UserProfileResponse> GetProfileAsync(Guid userUid);

        /// <summary>
        /// Altera dados do perfil do usuário
        /// </summary>
        /// <param name="userUid">Identificação do usuário</param>
        /// <param name="request">Alterações a registrar</param>
        /// <returns>Perfil do usuário</returns>
        Task<UserProfileResponse> UpdateProfileAsync(Guid userUid, UpdateUserProfileRequest request);

        /// <summary>
        /// ATualiza configurações da conta de usuário
        /// </summary>
        /// <param name="userUid">Identificação do usuário</param>
        /// <param name="request">Parâmetros a alterar</param>
        /// <returns>Configurações do usuário</returns>
        Task<UserSettingsDTO> UpdateSettingsAsync(Guid userUid, PatchUserSettingsRequest request);
    }
}
