using eTasks_server.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject] protected IAuthService AuthService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        protected string _email = string.Empty;
        protected string _password = string.Empty;
        protected bool _rememberMe = false;
        protected bool _showPassword = false;
        protected bool _isLoading = false;

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
