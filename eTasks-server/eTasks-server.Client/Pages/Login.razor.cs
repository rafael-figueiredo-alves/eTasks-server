using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject] protected HttpClient HttpClient { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        [SupplyParameterFromQuery]
        public string? ReturnUrl { get; set; }

        protected string _email = string.Empty;
        protected string _password = string.Empty;
        protected bool _rememberMe = false;
        protected bool _showPassword = false;
        protected bool _isLoading = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            var token = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            if (HasAdminRole(token))
            {
                HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                NavigationManager.NavigateTo(GetSafeReturnUrl(), replace: true);
                return;
            }

            await LogoutAsync();
            Snackbar.Add("Acesso restrito. Faça login com uma conta Administradora.", Severity.Warning);
            NavigationManager.NavigateTo("/login", replace: true);
        }

        protected void TogglePassword() => _showPassword = !_showPassword;

        protected async Task HandleLogin()
        {
            if (string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_password))
            {
                Snackbar.Add("Preencha todos os campos.", Severity.Warning);
                return;
            }

            _isLoading = true;
            try
            {
                var request = new LoginRequest
                {
                    Email = _email,
                    Password = _password,
                    UserAgent = Constants.WebAdminUserAgent
                };

                var response = await LoginAsync(request);
                if (response != null)
                {
                    Snackbar.Add("Login realizado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo(GetSafeReturnUrl(), replace: true);
                }
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                Snackbar.Add("Acesso restrito. Apenas administradores podem acessar o sistema.", Severity.Warning);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Falha ao entrar: {ex.Message}", Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private string GetSafeReturnUrl()
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl) || !Uri.IsWellFormedUriString(ReturnUrl, UriKind.Relative))
            {
                return "/";
            }

            return ReturnUrl;
        }

        private async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            HttpResponseMessage response;

            try
            {
                response = await HttpClient.PostAsJsonAsync("auth/login", request);
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

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse is null)
            {
                return null;
            }

            if (!HasAdminRole(loginResponse.Token))
            {
                throw new ApiException(HttpStatusCode.Forbidden, "{}", "Acesso restrito. Apenas administradores podem acessar o servidor.");
            }

            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", loginResponse.Token);
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", loginResponse.RefreshToken);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            return loginResponse;
        }

        private async Task LogoutAsync()
        {
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            HttpClient.DefaultRequestHeaders.Authorization = null;
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
    }
}
