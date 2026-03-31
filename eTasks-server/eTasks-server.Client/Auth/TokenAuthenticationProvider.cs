using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace eTasks_server.Client.Auth
{
    public class TokenAuthenticationProvider : AuthenticationStateProvider, IAuthToken
    {
        public static readonly string tokenKey = "authToken";
        public static readonly string refreshTokenKey = "refreshToken";

        private readonly HttpClient _httpClient;
        private readonly ITokenStorageService _tokenStorageService;
        private readonly IServiceProvider _serviceProvider;

        private static AuthenticationState NotAuthenticate => new(new ClaimsPrincipal(new ClaimsIdentity()));

        public TokenAuthenticationProvider(
            HttpClient httpClient,
            ITokenStorageService tokenStorageService,
            IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _tokenStorageService = tokenStorageService;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _tokenStorageService.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return NotAuthenticate;
            }

            if (GetExpirationFromToken(token) <= DateTime.UtcNow)
            {
                var authService = _serviceProvider.GetRequiredService<IAuthServices>();
                var refreshed = await authService.TryRefreshTokenAsync();

                if (refreshed is null || string.IsNullOrWhiteSpace(refreshed.Token))
                {
                    await _tokenStorageService.ClearTokensAsync();
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return NotAuthenticate;
                }

                token = refreshed.Token;
            }

            return CreateAuthenticationState(token);
        }

        public AuthenticationState CreateAuthenticationState(string Token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Token);

            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(Token), "jwt")));
        }

        public Task Login(string Token)
        {
            var authState = CreateAuthenticationState(Token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
            return Task.CompletedTask;
        }

        public Task Logout()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(Task.FromResult(NotAuthenticate));
            return Task.CompletedTask;
        }

        public async Task<string> GetUsername()
        {
            var claim = await GetClaimValueAsync(ClaimTypes.Name);
            return string.IsNullOrWhiteSpace(claim) ? "Unknown" : claim;
        }

        public async Task<string> GetUserID()
        {
            var claim = await GetClaimValueAsync(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(claim) ? "-1" : claim;
        }

        public async Task<string> GetEmail()
        {
            var claim = await GetClaimValueAsync(ClaimTypes.Email);
            return string.IsNullOrWhiteSpace(claim) ? "Unknown" : claim;
        }

        public async Task<string> GetRole()
        {
            var claim = await GetClaimValueAsync(ClaimTypes.Role);
            return claim ?? string.Empty;
        }

        public async Task<DateTime> GetExpiration()
        {
            var token = await _tokenStorageService.GetTokenAsync();
            return GetExpirationFromToken(token);
        }

        public async Task<string> GetUserAgent()
        {
            var claim = await GetClaimValueAsync(Constants.UserAgentClaimType);
            return claim ?? string.Empty;
        }

        private async Task<string?> GetClaimValueAsync(string claimType)
        {
            var token = await _tokenStorageService.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var claims = ParseClaimsFromJwt(token);
            return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes) ?? new Dictionary<string, object>();

            keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);
            if (roles is null && keyValuePairs.TryGetValue("role", out var roleAlias))
            {
                roles = roleAlias;
                keyValuePairs.Remove("role");
            }

            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);
                    foreach (var parsedRole in parsedRoles!)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }

        private static DateTime GetExpirationFromToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return DateTime.UtcNow.AddDays(-1);
            }

            var claims = ParseClaimsFromJwt(token);
            var expValue = claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp || x.Type == "exp")?.Value;

            if (string.IsNullOrWhiteSpace(expValue) || !long.TryParse(expValue, out var exp))
            {
                return DateTime.UtcNow.AddDays(-1);
            }

            return DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
        }
    }
}
