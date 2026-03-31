using eTasks_server.Models.Auth;

namespace eTasks_server.Client.Services.Interfaces
{
    public interface ITokenStorageService
    {
        Task StoreTokensAsync(LoginResponse response, bool persistInLocalStorage);
        Task<string?> GetTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task<bool> HasStoredSessionAsync();
        Task<bool> IsPersistentSessionAsync();
        Task ClearTokensAsync();
    }
}
