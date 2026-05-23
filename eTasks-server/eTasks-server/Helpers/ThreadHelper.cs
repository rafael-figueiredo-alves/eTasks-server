using MudBlazor;

namespace eTasks_server.Helpers
{
    public static class ThreadHelper
    {
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
