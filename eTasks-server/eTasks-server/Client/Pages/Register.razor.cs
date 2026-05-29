using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class RegisterBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] protected NavigationManager? NavigationManager { get; set; }
        [Inject] protected AuthenticationStateProvider? AuthenticationStateProvider { get; set; }
        [Inject] protected ISnackbar? Snackbar { get; set; }
        #endregion

        #region Parâmetros de URL (Query Params
        [SupplyParameterFromQuery] public string? Error { get; set; }
        #endregion

        #region Variáveis
        protected string _displayName = string.Empty;
        protected string _email = string.Empty;
        protected string _password = string.Empty;
        protected string _adminKey = string.Empty;
        protected bool _showPassword;
        protected bool _showAdminKey;
        protected bool _isLoading;

        protected bool _errorSnackShown;
        #endregion

        #region Métodos
        protected string ErrorMessage => string.IsNullOrWhiteSpace(Error) ? string.Empty : Error;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider!.GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                NavigationManager!.NavigateTo("/", replace: true);
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender || _errorSnackShown || string.IsNullOrWhiteSpace(ErrorMessage))
            {
                return;
            }

            _errorSnackShown = true;
            Snackbar!.Add(ErrorMessage, Severity.Error);
        }

        protected void BeginSubmit()
        {
            _isLoading = true;
        }
        #endregion
    }
}
