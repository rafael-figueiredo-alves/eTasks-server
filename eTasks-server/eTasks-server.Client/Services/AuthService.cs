using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;
using System.Security.Claims;

namespace eTasks_server.Client.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, IDialogService dialogService, IJSRuntime jsRuntime, AuthenticationStateProvider authStateProvider) 
            : base(httpClient, dialogService)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _authStateProvider = authStateProvider;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await PostAsync<LoginResponse>("/auth/login", request);
            
            if (response != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", response.Token);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", response.RefreshToken);
                
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(response.Token);
                
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Token);
            }
            
            return response;
        }

        public async Task<LoginResponse?> RefreshTokenAsync()
        {
            var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken)) return null;

            var request = new RefreshTokenRequest { RefreshToken = refreshToken, UserAgent = "Web" };
            var response = await PostAsync<LoginResponse>("/auth/refresh", request);

            if (response != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", response.Token);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", response.RefreshToken);

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Token);
            }
            else
            {
                await LogoutAsync();
            }

            return response;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return await _authStateProvider.GetAuthenticationStateAsync();
        }
    }
}
