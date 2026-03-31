using eTasks_server.Client.Auth;
using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace eTasks_server.Client.Services
{
    public class AuthService : IAuthServices
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenStorageService _tokenStorageService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthService(
            HttpClient httpClient,
            ITokenStorageService tokenStorageService,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _tokenStorageService = tokenStorageService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, bool rememberMe)
        {
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsJsonAsync("auth/login", request);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new ApiException(response.StatusCode, content, $"Erro ao consumir API: {response.ReasonPhrase}");
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>()
                ?? throw new ApiException(HttpStatusCode.InternalServerError, null, "A API retornou uma resposta de login inválida.");

            ValidateAdminAccess(loginResponse.Token);

            await _tokenStorageService.StoreTokensAsync(loginResponse, rememberMe);
            await ((TokenAuthenticationProvider)_authenticationStateProvider).Login(loginResponse.Token);

            return loginResponse;
        }

        public async Task LogoutAsync()
        {
            await _tokenStorageService.ClearTokensAsync();
            await ((TokenAuthenticationProvider)_authenticationStateProvider).Logout();
        }

        public async Task<LoginResponse> RefreshTokenAsync()
        {
            var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, null, "Sessão inválida. Faça login novamente.");
            }

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsJsonAsync("auth/refresh", new RefreshTokenRequest
                {
                    RefreshToken = refreshToken,
                    UserAgent = Constants.WebAdminUserAgent
                });
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                await LogoutAsync();
                throw new ApiException(response.StatusCode, content, $"Erro ao consumir API: {response.ReasonPhrase}");
            }

            var refreshResponse = await response.Content.ReadFromJsonAsync<LoginResponse>()
                ?? throw new ApiException(HttpStatusCode.InternalServerError, null, "A API retornou uma resposta de renovação inválida.");

            ValidateAdminAccess(refreshResponse.Token);

            var keepPersistentSession = await _tokenStorageService.IsPersistentSessionAsync();
            await _tokenStorageService.StoreTokensAsync(refreshResponse, keepPersistentSession);
            await ((TokenAuthenticationProvider)_authenticationStateProvider).Login(refreshResponse.Token);

            return refreshResponse;
        }

        public async Task<LoginResponse?> TryRefreshTokenAsync()
        {
            try
            {
                return await RefreshTokenAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> EnsureValidTokenAsync()
        {
            var token = await _tokenStorageService.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (!IsExpired(token))
            {
                await ((TokenAuthenticationProvider)_authenticationStateProvider).Login(token);
                return true;
            }

            return await TryRefreshTokenAsync() is not null;
        }

        private static void ValidateAdminAccess(string token)
        {
            if (!HasAdminRole(token))
            {
                throw new ApiException(HttpStatusCode.Forbidden, "{}", "Acesso restrito. Apenas administradores podem acessar o servidor.");
            }
        }

        private static bool HasAdminRole(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
                var userAgentClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == Constants.UserAgentClaimType);

                return roleClaim?.Value == "Admin"
                    && userAgentClaim?.Value == Constants.WebAdminUserAgent;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp || c.Type == "exp")?.Value;

                if (string.IsNullOrWhiteSpace(expClaim) || !long.TryParse(expClaim, out var exp))
                {
                    return true;
                }

                return DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime <= DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}
