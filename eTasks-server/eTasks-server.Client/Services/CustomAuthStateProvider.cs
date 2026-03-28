using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace eTasks_server.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return new AuthenticationState(_anonymous);
                }
                var principal = CreateClaimsPrincipalFromToken(token);
                if (principal == null)
                {
                    await ClearSessionAsync();
                    return new AuthenticationState(_anonymous);
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return new AuthenticationState(principal);
            }
            catch
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return new AuthenticationState(_anonymous);
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var authenticatedUser = CreateClaimsPrincipalFromToken(token);
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser ?? _anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        private ClaimsPrincipal? CreateClaimsPrincipalFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return null;
                }

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
                if (!string.Equals(roleClaim?.Value, "Admin", StringComparison.Ordinal))
                {
                    return null;
                }

                var userAgentClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == Constants.UserAgentClaimType);
                if (!string.Equals(userAgentClaim?.Value, Constants.WebAdminUserAgent, StringComparison.Ordinal))
                {
                    return null;
                }

                var nameClaimType = jwtToken.Claims.Any(c => c.Type == ClaimTypes.Name) ? ClaimTypes.Name : "name";
                var roleClaimType = jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role) ? ClaimTypes.Role : "role";

                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt", nameClaimType, roleClaimType);
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        private async Task ClearSessionAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
