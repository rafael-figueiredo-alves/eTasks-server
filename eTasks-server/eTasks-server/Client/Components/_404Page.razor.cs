using Microsoft.AspNetCore.Components;

namespace eTasks_server.Client.Components
{
    public partial class _404Page
    {
        #region Serviços Injetados
        [Inject] private NavigationManager Nav { get; set; } = default!;
        #endregion

        #region Variáveis
        private string Background { get; set; } = "";
        private string LineStroke { get; set; } = "#0E0620";
        private string ImageFill { get; set; } = "#FFFFFF";
        private string TextColor { get; set; } = "black";
        #endregion

        #region Métodos
        private void GoBackToHomePage() => Nav.NavigateTo("/");
        #endregion
    }
}
