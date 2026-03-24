using eTasks_server.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> RefreshTokenAsync();
        Task LogoutAsync();
        Task<AuthenticationState> GetAuthenticationStateAsync();
    }
}
