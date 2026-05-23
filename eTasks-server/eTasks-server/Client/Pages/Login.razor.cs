using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject] protected NavigationManager? NavigationManager { get; set; }
        [Inject] protected AuthenticationStateProvider? AuthenticationStateProvider { get; set; }
        [Inject] protected ISnackbar? Snackbar { get; set; }

        [SupplyParameterFromQuery] public string? ReturnUrl { get; set; }
        [SupplyParameterFromQuery] public string? Error { get; set; }
        [SupplyParameterFromQuery] public string? Success { get; set; }

        protected string _email = string.Empty;
        protected string _password = string.Empty;
        protected bool _rememberMe = true;
        protected bool _showPassword;
        protected bool _isLoading;

        protected bool _errorSnackShown;
        protected bool _successSnackShown;
        protected string ErrorMessage => string.IsNullOrWhiteSpace(Error) ? string.Empty : Error;
        protected string SuccessMessage => string.IsNullOrWhiteSpace(Success) ? string.Empty : Success;
        protected string LoginAction => $"/api/v2/web-auth/login?returnUrl={Uri.EscapeDataString(GetSafeReturnUrl())}";

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider!.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                NavigationManager!.NavigateTo(GetSafeReturnUrl(), replace: true);
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && !_errorSnackShown && !string.IsNullOrWhiteSpace(ErrorMessage))
            {
                _errorSnackShown = true;
                Snackbar!.Add(ErrorMessage, Severity.Error);
            }

            if (firstRender && !_successSnackShown && !string.IsNullOrWhiteSpace(SuccessMessage))
            {
                _successSnackShown = true;
                Snackbar!.Add(SuccessMessage, Severity.Success);
            }
        }

        protected void BeginSubmit()
        {
            _isLoading = true;
        }

        protected string GetSafeReturnUrl()
        {
            if (string.IsNullOrWhiteSpace(ReturnUrl) || !Uri.IsWellFormedUriString(ReturnUrl, UriKind.Relative))
            {
                return "/";
            }

            return ReturnUrl;
        }
    }
}
