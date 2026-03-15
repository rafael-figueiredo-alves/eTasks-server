using eTasks_server.Client.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Services.Extensions
{
    public static class DialogServiceExtensions
    {
        extension(IDialogService dialogService)
        {
            public async Task ShowError(string message, string title = "Error")
            {
                var options = new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                };

                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                };

                await dialogService.ShowAsync<ErrorDialog>(title, parameters, options);
            }

            public async Task ShowConfirm(string message, string title = "Error", EventCallback? OnConfirm = null, EventCallback? OnCancel = null)
            {
                var options = new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                };

                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                    ["OnConfirm"] = OnConfirm,
                    ["OnCancel"] = OnCancel
                };

                await dialogService.ShowAsync<ConfirmDialog>(title, parameters, options);
            }
        }
    }
}
