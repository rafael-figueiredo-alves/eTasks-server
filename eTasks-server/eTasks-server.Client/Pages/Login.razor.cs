using Microsoft.AspNetCore.Components;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        protected string _email = string.Empty;
        protected string _password = string.Empty;
        protected bool _rememberMe = false;
        protected bool _showPassword = false;
        protected bool _isLoading = false;

        protected void TogglePassword() => _showPassword = !_showPassword;

        protected async Task HandleLogin()
        {
            _isLoading = true;
            // Simula chamada à API — substitua pela sua lógica de autenticação
            await Task.Delay(1500);
            _isLoading = false;
            // NavigationManager.NavigateTo("/dashboard");
        }
    }
}
