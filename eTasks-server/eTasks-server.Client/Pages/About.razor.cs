using Microsoft.AspNetCore.Components;

namespace eTasks_server.Client.Pages
{
    public class AboutBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        #endregion

        #region Variáveis
        protected string? VersionDetails { get; private set; }
        private HttpClient? Http;
        #endregion

        #region Métodos
        protected override async Task OnInitializedAsync()
        {
            Http = new HttpClient() { BaseAddress = new Uri(NavigationManager.BaseUri) };

            VersionDetails = await Http.GetStringAsync("About.txt");
        }
        #endregion
    }
}
