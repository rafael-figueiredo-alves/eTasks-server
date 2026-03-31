using eTasks_server.Models.Auth;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IAuthServices
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, bool rememberMe);
        Task LogoutAsync();
        Task<LoginResponse> RefreshTokenAsync();
        Task<LoginResponse?> TryRefreshTokenAsync();
        Task<bool> EnsureValidTokenAsync();
    }
}
