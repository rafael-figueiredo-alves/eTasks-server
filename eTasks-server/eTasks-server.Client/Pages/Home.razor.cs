using eTasks_server.Client.Components;
using eTasks_server.Client.Services.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class HomeBase : ComponentBase
    {
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        protected async Task Teste()
        {
            await DialogService.ShowConfirm("Esse é um teste da função para exibir uma confirmação.", "Mensagem de confirmação", OnConfirm: EventCallback.Factory.Create(this, async () =>
            {
                await DialogService.ShowError("Você confirmou a ação!", "Confirmação");
            }), OnCancel: EventCallback.Factory.Create(this, async () =>
            {
                await DialogService.ShowError("Você cancelou a ação!", "Cancelamento");
            }));

            await DialogService.ShowError("Esse é um teste da função para exibir um erro.", "Mensagem de erro");
        }
    }
}
