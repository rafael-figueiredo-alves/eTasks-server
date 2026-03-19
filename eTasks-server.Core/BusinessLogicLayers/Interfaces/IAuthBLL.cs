using System.Threading.Tasks;
using eTasks_server.Models.Auth;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface IAuthBLL
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ChangePasswordAsync(Guid userUid, ChangePasswordRequest request);
        Task<bool> ConfirmEmailAsync(string token);
    }
}
