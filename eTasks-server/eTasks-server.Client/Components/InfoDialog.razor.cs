using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Components
{
    public class InfoDialogBase : ComponentBase
    {
        [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;

        [Parameter] public string Title { get; set; } = "Erro";
        [Parameter] public string Message { get; set; } = "";

        protected void Close() => Dialog.Close();
    }
}
