using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected Services.Interfaces.IAuthServices AuthServices { get; set; } = default!;
        [Inject] protected Auth.IAuthToken AuthToken { get; set; } = default!;

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

            if (!await AuthServices.EnsureValidTokenAsync())
            {
                return;
            }

            if (await AuthToken.GetRole() == "Admin" && await AuthToken.GetUserAgent() == Constants.WebAdminUserAgent)
            {
                NavigationManager.NavigateTo("/", replace: true);
                return;
            }

            await AuthServices.LogoutAsync();
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

                await AuthServices.LoginAsync(request, _rememberMe);
                Snackbar.Add("Login realizado com sucesso!", Severity.Success);
                NavigationManager.NavigateTo("/", replace: true);
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
    }
}
