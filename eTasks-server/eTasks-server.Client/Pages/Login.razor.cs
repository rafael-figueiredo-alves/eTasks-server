using eTasks_server.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using eTasks_server.Client.Services;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject] protected IAuthService AuthService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
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
            if (firstRender)
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    if (authState.User.IsInRole("Admin"))
                    {
                        if (AuthStateProvider is CustomAuthStateProvider customProvider)
                        {
                            var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                customProvider.NotifyUserAuthentication(token);
                            }
                        }
                        NavigationManager.NavigateTo(string.IsNullOrEmpty(ReturnUrl) ? "/" : ReturnUrl);
                    }
                    else
                    {
                        await AuthService.LogoutAsync();
                    }
                }
            }
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
                var request = new eTasks_server.Models.Auth.LoginRequest 
                { 
                    Email = _email, 
                    Password = _password,
                    UserAgent = "Web"
                };

                var response = await AuthService.LoginAsync(request);
                if (response != null)
                {
                    Snackbar.Add("Login realizado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/");
                }
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
    }
}
