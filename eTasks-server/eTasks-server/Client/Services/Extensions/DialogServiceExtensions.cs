using eTasks_server.Client.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace eTasks_server.Client.Services.Extensions
{
    /// <summary>
    /// Serviço de extensão para o IDialogService, fornecendo métodos convenientes para exibir diálogos de erro, informação e confirmação.
    /// </summary>
    public static class DialogServiceExtensions
    {
        extension(IDialogService dialogService)
        {
            /// <summary>
            /// Pega configurações padrão para os diálogos, garantindo uma aparência consistente e funcionalidade adequada.
            /// </summary>
            /// <returns></returns>
            private static DialogOptions GetDefaultOptions()
            {
                return new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.Small,
                    FullWidth = true
                };
            }

            /// <summary>
            /// Exibe um diálogo de erro com uma mensagem personalizada e um título opcional. O diálogo inclui um botão de fechar para que o usuário possa fechá-lo facilmente.
            /// </summary>
            /// <param name="message">A mensagem a ser exibida no diálogo de erro.</param>
            /// <param name="title">O título do diálogo de erro. O padrão é "Error".</param>
            /// <returns></returns>
            public async Task ShowError(string message, string title = "Error")
            {
                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                };

                await dialogService.ShowAsync<ErrorDialog>(title, parameters, IDialogService.GetDefaultOptions());
            }

            /// <summary>
            /// Exibe um diálogo de informação com uma mensagem personalizada e um título opcional. O diálogo inclui um botão de fechar para que o usuário possa fechá-lo facilmente.
            /// </summary>
            /// <param name="message">A mensagem a ser exibida no diálogo de informação.</param>
            /// <param name="title">O título do diálogo de informação. O padrão é "Info".</param>
            /// <returns></returns>
            public async Task ShowInfo(string message, string title = "Error")
            {
                var parameters = new DialogParameters
                {
                    ["Title"] = title,
                    ["Message"] = message,
                };

                await dialogService.ShowAsync<InfoDialog>(title, parameters, IDialogService.GetDefaultOptions());
            }

            /// <summary>
            /// Exibe um diálogo de confirmação com uma mensagem personalizada, um título opcional e callbacks para as ações de confirmação e cancelamento. O diálogo inclui botões para confirmar ou cancelar a ação, permitindo que o usuário tome uma decisão informada.
            /// </summary>
            /// <param name="message">A mensagem a ser exibida no diálogo de confirmação.</param>
            /// <param name="title">O título do diálogo de confirmação. O padrão é "Confirm".</param>
            /// <param name="OnConfirm">Callback a ser executado quando o usuário confirmar a ação.</param>
            /// <param name="OnCancel">Callback a ser executado quando o usuário cancelar a ação.</param>
            /// <returns></returns>
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
