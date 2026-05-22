using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Components
{
    public class SetPasswordDialogBase : ComponentBase
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

        protected string? Password { get; set; }

        protected void Submit() => MudDialog.Close(DialogResult.Ok(Password));
        protected void Cancel() => MudDialog.Cancel();
    }
}
