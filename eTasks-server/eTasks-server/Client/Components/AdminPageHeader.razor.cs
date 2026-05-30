using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Components
{
    public class AdminPageHeaderBase : ComponentBase
    {
        #region Serviços Injetados
        [Inject] protected NavigationManager? Navigation { get; set; }
        #endregion

        #region Parâmetros
        [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
        [Parameter] public string? Subtitle { get; set; }
        [Parameter] public string BackUrl { get; set; } = "/";
        [Parameter] public string BackTooltip { get; set; } = "Voltar";
        [Parameter] public Typo TitleTypo { get; set; } = Typo.h4;
        [Parameter] public string Class { get; set; } = "mb-6";
        [Parameter] public RenderFragment? ChildContent { get; set; }
        #endregion

        #region Métodos
        protected void NavigateBack() => Navigation?.NavigateTo(BackUrl);
        #endregion
    }
}
