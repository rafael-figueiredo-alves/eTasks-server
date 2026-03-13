using eTasks_server.Client.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Pages
{
    public class HomeBase : ComponentBase
    {
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        protected async Task Teste()
        {
            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            };

            var parameters = new DialogParameters
            {
                ["Title"] = "Erro Teste",
                ["Message"] = "Mensagem de erro teste"
            };

            await DialogService.ShowAsync<ErrorDialog>("Erro Teste", parameters, options);
        }
    }
}
