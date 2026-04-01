using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected Services.Interfaces.IWebAuthService WebAuthService { get; set; } = default!;

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }

        protected string _email = string.Empty;
        protected string _password = string.Empty;
        protected bool _rememberMe = true;
        protected bool _showPassword = false;
        protected bool _isLoading = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                NavigationManager.NavigateTo(GetSafeReturnUrl(), replace: true);
            }
        }

        [Inject] protected Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

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
                var request = new WebLoginRequest
                {
                    Email = _email,
                    Password = _password,
                    RememberMe = _rememberMe
                };

                await WebAuthService.LoginAsync(request);
                Snackbar.Add("Login realizado com sucesso!", Severity.Success);
                NavigationManager.NavigateTo(GetSafeReturnUrl(), forceLoad: true);
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
    }
}
