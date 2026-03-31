using eTasks_server.Client.Auth;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Auth;
using Microsoft.JSInterop;

namespace eTasks_server.Client.Services
{
    public class TokenStorageService : ITokenStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public TokenStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task StoreTokensAsync(LoginResponse response, bool persistInLocalStorage)
        {
            var targetStorage = persistInLocalStorage ? "localStorage" : "sessionStorage";
            var secondaryStorage = persistInLocalStorage ? "sessionStorage" : "localStorage";

            await ClearStorageAsync(secondaryStorage);
            await _jsRuntime.InvokeVoidAsync($"{targetStorage}.setItem", TokenAuthenticationProvider.tokenKey, response.Token);
            await _jsRuntime.InvokeVoidAsync($"{targetStorage}.setItem", TokenAuthenticationProvider.refreshTokenKey, response.RefreshToken);
        }

        public async Task<string?> GetTokenAsync()
        {
            var localToken = await TryGetStorageValueAsync("localStorage", TokenAuthenticationProvider.tokenKey);
            if (!string.IsNullOrWhiteSpace(localToken))
            {
                return localToken;
            }

            return await TryGetStorageValueAsync("sessionStorage", TokenAuthenticationProvider.tokenKey);
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            var localToken = await TryGetStorageValueAsync("localStorage", TokenAuthenticationProvider.refreshTokenKey);
            if (!string.IsNullOrWhiteSpace(localToken))
            {
                return localToken;
            }

            return await TryGetStorageValueAsync("sessionStorage", TokenAuthenticationProvider.refreshTokenKey);
        }

        public async Task<bool> HasStoredSessionAsync()
        {
            return !string.IsNullOrWhiteSpace(await GetTokenAsync())
                || !string.IsNullOrWhiteSpace(await GetRefreshTokenAsync());
        }

        public async Task<bool> IsPersistentSessionAsync()
        {
            var localToken = await TryGetStorageValueAsync("localStorage", TokenAuthenticationProvider.tokenKey);
            var localRefresh = await TryGetStorageValueAsync("localStorage", TokenAuthenticationProvider.refreshTokenKey);

            return !string.IsNullOrWhiteSpace(localToken) && !string.IsNullOrWhiteSpace(localRefresh);
        }

        public async Task ClearTokensAsync()
        {
            await ClearStorageAsync("localStorage");
            await ClearStorageAsync("sessionStorage");
        }

        private async Task ClearStorageAsync(string storageName)
        {
            await TryRemoveStorageValueAsync(storageName, TokenAuthenticationProvider.tokenKey);
            await TryRemoveStorageValueAsync(storageName, TokenAuthenticationProvider.refreshTokenKey);
        }

        private async Task<string?> TryGetStorageValueAsync(string storageName, string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>($"{storageName}.getItem", key);
            }
            catch (InvalidOperationException ex) when (IsPrerenderInteropException(ex))
            {
                return null;
            }
            catch (JSDisconnectedException)
            {
                return null;
            }
        }

        private async Task TryRemoveStorageValueAsync(string storageName, string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync($"{storageName}.removeItem", key);
            }
            catch (InvalidOperationException ex) when (IsPrerenderInteropException(ex))
            {
            }
            catch (JSDisconnectedException)
            {
            }
        }

        private static bool IsPrerenderInteropException(InvalidOperationException ex)
        {
            return ex.Message.Contains("statically rendered", StringComparison.OrdinalIgnoreCase)
                || ex.Message.Contains("prerender", StringComparison.OrdinalIgnoreCase);
        }
    }
}
