using eTasks_server.Client.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Services.Extensions
{
    public static class DialogServiceExtensions
    {
        extension(IDialogService dialogService)
        {
            private static DialogOptions GetDefaultOptions()
            {
                return new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                };
            }

            public async Task ShowError(string message, string title = "Error")
            {
                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                };

                await dialogService.ShowAsync<ErrorDialog>(title, parameters, IDialogService.GetDefaultOptions());
            }

            public async Task ShowInfo(string message, string title = "Error")
            {
                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                };

                await dialogService.ShowAsync<InfoDialog>(title, parameters, IDialogService.GetDefaultOptions());
            }

            public async Task ShowConfirm(string message, string title = "Error", EventCallback? OnConfirm = null, EventCallback? OnCancel = null)
            {                
                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                    ["OnConfirm"] = OnConfirm,
                    ["OnCancel"] = OnCancel
                };

                await dialogService.ShowAsync<ConfirmDialog>(title, parameters, IDialogService.GetDefaultOptions());
            }
        }
    }
}
