using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Components
{
    public class InfoDialogBase : ComponentBase
    {
        #region Parâmetros
        [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;

        [Parameter] public string Title { get; set; } = "Erro";
        [Parameter] public string Message { get; set; } = "";
        #endregion

        #region Métodos
        protected void Close() => Dialog.Close();
        #endregion
    }
}
