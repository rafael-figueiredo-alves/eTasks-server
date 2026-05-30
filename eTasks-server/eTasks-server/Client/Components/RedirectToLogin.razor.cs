using Microsoft.AspNetCore.Components;

namespace eTasks_server.Client.Components
{
    public class RedirectToLoginBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] protected NavigationManager? NavigationManager { get; set; }
        #endregion

        #region Métodos
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            var currentPath = "/" + NavigationManager?.ToBaseRelativePath(NavigationManager.Uri);

            if (string.IsNullOrWhiteSpace(currentPath) || currentPath == "/")
            {
                currentPath = "/";
            }

            var encodedReturnUrl = Uri.EscapeDataString(currentPath);

            NavigationManager?.NavigateTo($"/login?returnUrl={encodedReturnUrl}", replace: true);
        }
        #endregion
    }
}
