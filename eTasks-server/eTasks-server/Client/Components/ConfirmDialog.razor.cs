using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Components
{
    public class ConfirmDialogBase : ComponentBase
    {
        #region Parâmetros
        [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;

        [Parameter] public string Title { get; set; } = "Confirmação";
        [Parameter] public string Message { get; set; } = "";
        [Parameter] public EventCallback OnConfirm { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        #endregion

        #region Métodos
        protected async Task Confirm()
        {
            if(OnConfirm.HasDelegate)
                await OnConfirm.InvokeAsync();

            Dialog.Close();
        }

        protected async Task Cancel()
        {
            if(OnCancel.HasDelegate)
                await OnCancel.InvokeAsync();

            Dialog.Close();
        }
        #endregion
    }
}
