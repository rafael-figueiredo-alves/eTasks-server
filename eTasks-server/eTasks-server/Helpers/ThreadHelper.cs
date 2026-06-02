using MudBlazor;

namespace eTasks_server.Helpers
{
    // Classe auxiliar para executar ações assíncronas com tratamento de erros e controle de estado de "busy".
    public static class ThreadHelper
    {
        /// <summary>
        /// Executa uma ação assíncrona, exibindo um indicador de "busy" e tratando erros de forma centralizada.
        /// </summary>
        /// <param name="action">Ação a executar</param>
        /// <param name="defaultErrorMessage">Mensagem de erro padrão</param>
        /// <param name="snackbar">Instância do Snackbar para exibir mensagens</param>
        /// <param name="setBusy">Ação para definir o estado de "busy"</param>
        /// <param name="setStatus">Ação para definir o status</param>
        /// <returns></returns>
        public static async Task ExecuteBusyAsync(Func<Task> action, string defaultErrorMessage, ISnackbar snackbar, Action<bool>? setBusy = null, Action<string, Severity>? setStatus = null)
        {
            try
            { 
                setBusy?.Invoke(true);
                await action();
            }
            catch (Exception ex)
            {
                var message = string.IsNullOrWhiteSpace(ex.Message) ? defaultErrorMessage : ex.Message;
                setStatus?.Invoke(message, Severity.Error);
                snackbar.Add(message, Severity.Error);
            }
            finally
            {
                setBusy?.Invoke(false);
            }
        }
    }
}
