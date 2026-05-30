using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Components
{
    public class SetPasswordDialogBase : ComponentBase
    {
        #region Parâmetros
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
        #endregion

        #region Variáveis
        protected string? Password { get; set; }
        #endregion

        #region Métodos
        protected void Submit() => MudDialog.Close(DialogResult.Ok(Password));
        protected void Cancel() => MudDialog.Cancel();
        #endregion
    }
}
